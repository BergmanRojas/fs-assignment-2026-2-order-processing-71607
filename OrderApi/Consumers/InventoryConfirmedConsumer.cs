using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts.Events;

namespace OrderApi.Consumers;

public class InventoryConfirmedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InventoryConfirmedConsumer> _logger;

    public InventoryConfirmedConsumer(IServiceScopeFactory scopeFactory, ILogger<InventoryConfirmedConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";

        var factory = new ConnectionFactory
        {
            HostName = rabbitMqHost
        };

        var connection = await factory.CreateConnectionAsync(stoppingToken);
        var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(
            exchange: "inventory-confirmed-exchange",
            type: ExchangeType.Fanout,
            durable: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: "inventory-confirmed-orderapi",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: "inventory-confirmed-orderapi",
            exchange: "inventory-confirmed-exchange",
            routingKey: string.Empty,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                var inventoryConfirmed = JsonSerializer.Deserialize<InventoryConfirmed>(json);

                if (inventoryConfirmed is not null)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

                    var order = await db.Orders
                        .FirstOrDefaultAsync(o => o.OrderId == inventoryConfirmed.OrderId, stoppingToken);

                    if (order is not null)
                    {
                        order.InventoryConfirmedAt = inventoryConfirmed.ConfirmedAt;
                        order.Status = inventoryConfirmed.IsInStock
                            ? "Inventory Confirmed"
                            : "Out of Stock";

                        await db.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation(
                            "Order {OrderId} updated after inventory confirmation. Status: {Status}",
                            order.OrderId,
                            order.Status);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Order {OrderId} not found when processing InventoryConfirmed",
                            inventoryConfirmed.OrderId);
                    }
                }

                await channel.BasicAckAsync(
                    ea.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing InventoryConfirmed message");

                await channel.BasicNackAsync(
                    ea.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: "inventory-confirmed-orderapi",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("OrderApi is waiting for inventory-confirmed messages...");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
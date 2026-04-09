using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts.Events;

namespace OrderApi.Consumers;

public class ShippingCreatedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ShippingCreatedConsumer> _logger;

    public ShippingCreatedConsumer(IServiceScopeFactory scopeFactory, ILogger<ShippingCreatedConsumer> logger)
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
            exchange: "shipping-created-exchange",
            type: ExchangeType.Fanout,
            durable: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: "shipping-created-orderapi",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: "shipping-created-orderapi",
            exchange: "shipping-created-exchange",
            routingKey: string.Empty,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                var shippingCreated = JsonSerializer.Deserialize<ShippingCreated>(json);

                if (shippingCreated is not null)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

                    var order = await db.Orders
                        .FirstOrDefaultAsync(o => o.OrderId == shippingCreated.OrderId, stoppingToken);

                    if (order is not null)
                    {
                        order.ShippingCreatedAt = shippingCreated.DispatchDate;
                        order.ShipmentReference = shippingCreated.ShipmentReference;
                        order.Status = "Completed";

                        await db.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation(
                            "Order {OrderId} marked as Completed after shipping creation",
                            order.OrderId);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Order {OrderId} not found when processing ShippingCreated",
                            shippingCreated.OrderId);
                    }
                }

                await channel.BasicAckAsync(
                    ea.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing ShippingCreated message");

                await channel.BasicNackAsync(
                    ea.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: "shipping-created-orderapi",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("OrderApi is waiting for shipping-created messages...");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
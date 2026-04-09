using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts.Events;
using InventoryService.Producers;
using Microsoft.Extensions.Logging;

namespace InventoryService.Consumers;

public class OrderSubmittedConsumer : BackgroundService
{
    private readonly RabbitMqPublisher _publisher;
    private readonly ILogger<OrderSubmittedConsumer> _logger;

    public OrderSubmittedConsumer(RabbitMqPublisher publisher, ILogger<OrderSubmittedConsumer> logger)
    {
        _publisher = publisher;
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

        await channel.QueueDeclareAsync(
            queue: "order-submitted",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            var orderSubmitted = JsonSerializer.Deserialize<OrderSubmitted>(json);

            _logger.LogInformation("InventoryService received OrderSubmitted message");
            _logger.LogInformation("OrderId: {OrderId}", orderSubmitted?.OrderId);
            _logger.LogInformation("CustomerId: {CustomerId}", orderSubmitted?.CustomerId);
            _logger.LogInformation("SubmittedAt: {SubmittedAt}", orderSubmitted?.SubmittedAt);

            if (orderSubmitted is not null)
            {
                var inventoryConfirmed = new InventoryConfirmed
                {
                    OrderId = orderSubmitted.OrderId,
                    CustomerId = orderSubmitted.CustomerId,
                    ConfirmedAt = DateTime.UtcNow,
                    IsInStock = true
                };

                await _publisher.PublishInventoryConfirmed(inventoryConfirmed);
            }

            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
        };

        await channel.BasicConsumeAsync(
            queue: "order-submitted",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("InventoryService is waiting for messages...");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
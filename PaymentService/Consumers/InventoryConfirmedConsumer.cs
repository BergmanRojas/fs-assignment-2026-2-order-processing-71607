using System.Text;
using System.Text.Json;
using PaymentService.Producers;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace PaymentService.Consumers;

public class InventoryConfirmedConsumer : BackgroundService
{
    private readonly RabbitMqPublisher _publisher;
    private readonly ILogger<InventoryConfirmedConsumer> _logger;

    public InventoryConfirmedConsumer(RabbitMqPublisher publisher, ILogger<InventoryConfirmedConsumer> logger)
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

        await channel.ExchangeDeclareAsync(
            exchange: "inventory-confirmed-exchange",
            type: ExchangeType.Fanout,
            durable: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: "inventory-confirmed-payment",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: "inventory-confirmed-payment",
            exchange: "inventory-confirmed-exchange",
            routingKey: string.Empty,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            var inventoryConfirmed = JsonSerializer.Deserialize<InventoryConfirmed>(json);

            _logger.LogInformation("PaymentService received InventoryConfirmed message");
            _logger.LogInformation("OrderId: {OrderId}", inventoryConfirmed?.OrderId);
            _logger.LogInformation("CustomerId: {CustomerId}", inventoryConfirmed?.CustomerId);
            _logger.LogInformation("ConfirmedAt: {ConfirmedAt}", inventoryConfirmed?.ConfirmedAt);
            _logger.LogInformation("IsInStock: {IsInStock}", inventoryConfirmed?.IsInStock);

            if (inventoryConfirmed is not null && inventoryConfirmed.IsInStock)
            {
                var paymentApproved = new PaymentApproved
                {
                    OrderId = inventoryConfirmed.OrderId,
                    CustomerId = inventoryConfirmed.CustomerId,
                    ApprovedAt = DateTime.UtcNow,
                    IsApproved = true
                };

                await _publisher.PublishPaymentApproved(paymentApproved);
            }

            await channel.BasicAckAsync(
                ea.DeliveryTag,
                multiple: false,
                cancellationToken: stoppingToken);
        };

        await channel.BasicConsumeAsync(
            queue: "inventory-confirmed-payment",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("PaymentService is waiting for messages...");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
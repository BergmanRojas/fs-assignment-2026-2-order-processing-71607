using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts.Events;
using ShippingService.Producers;
using Microsoft.Extensions.Logging;

namespace ShippingService.Consumers;

public class PaymentApprovedConsumer : BackgroundService
{
    private readonly RabbitMqPublisher _publisher;
    private readonly ILogger<PaymentApprovedConsumer> _logger;

    public PaymentApprovedConsumer(RabbitMqPublisher publisher, ILogger<PaymentApprovedConsumer> logger)
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
            queue: "payment-approved",
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

            var paymentApproved = JsonSerializer.Deserialize<PaymentApproved>(json);

            _logger.LogInformation("ShippingService received PaymentApproved message");
            _logger.LogInformation("OrderId: {OrderId}", paymentApproved?.OrderId);
            _logger.LogInformation("CustomerId: {CustomerId}", paymentApproved?.CustomerId);
            _logger.LogInformation("ApprovedAt: {ApprovedAt}", paymentApproved?.ApprovedAt);
            _logger.LogInformation("IsApproved: {IsApproved}", paymentApproved?.IsApproved);

            if (paymentApproved is not null && paymentApproved.IsApproved)
            {
                var shippingCreated = new ShippingCreated
                {
                    OrderId = paymentApproved.OrderId,
                    CustomerId = paymentApproved.CustomerId,
                    ShipmentReference = $"SHIP-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                    DispatchDate = DateTime.UtcNow.AddDays(1)
                };

                await _publisher.PublishShippingCreated(shippingCreated);
            }

            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
        };

        await channel.BasicConsumeAsync(
            queue: "payment-approved",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("ShippingService is waiting for messages...");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
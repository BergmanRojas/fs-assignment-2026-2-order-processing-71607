using System.Text;
using System.Text.Json;
using OrderApi.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Contracts.Events;
using Microsoft.Extensions.Logging;

namespace OrderApi.Consumers;

public class ShippingCreatedConsumer : BackgroundService
{
    private readonly OrderStore _orderStore;
    private readonly ILogger<ShippingCreatedConsumer> _logger;

    public ShippingCreatedConsumer(OrderStore orderStore, ILogger<ShippingCreatedConsumer> logger)
    {
        _orderStore = orderStore;
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
            queue: "shipping-created",
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

            var shippingCreated = JsonSerializer.Deserialize<ShippingCreated>(json);

            if (shippingCreated is not null)
            {
                _orderStore.UpdateStatus(shippingCreated.OrderId, "Completed");

                _logger.LogInformation("Order {OrderId} marked as Completed after shipping creation", shippingCreated.OrderId);
            }

            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
        };

        await channel.BasicConsumeAsync(
            queue: "shipping-created",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("OrderApi is waiting for shipping-created messages...");

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
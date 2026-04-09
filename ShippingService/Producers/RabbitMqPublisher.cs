using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Shared.Contracts.Events;

namespace ShippingService.Producers;

public class RabbitMqPublisher
{
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(ILogger<RabbitMqPublisher> logger)
    {
        _logger = logger;
    }

    public async Task PublishShippingCreated(ShippingCreated shippingCreated)
    {
        var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";

        var factory = new ConnectionFactory
        {
            HostName = rabbitMqHost
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: "shipping-created-exchange",
            type: ExchangeType.Fanout,
            durable: false,
            autoDelete: false,
            arguments: null);

        var json = JsonSerializer.Serialize(shippingCreated);
        var body = Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(
            exchange: "shipping-created-exchange",
            routingKey: string.Empty,
            body: body);

        _logger.LogInformation(
            "ShippingCreated published for OrderId: {OrderId} with ShipmentReference: {ShipmentReference}",
            shippingCreated.OrderId,
            shippingCreated.ShipmentReference);
    }
}
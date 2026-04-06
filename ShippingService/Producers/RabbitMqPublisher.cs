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
        var factory = new ConnectionFactory
        {
            HostName = "localhost"
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "shipping-created",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var json = JsonSerializer.Serialize(shippingCreated);
        var body = Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: "shipping-created",
            body: body);

        _logger.LogInformation(
            "ShippingCreated published for OrderId: {OrderId} with ShipmentReference: {ShipmentReference}",
            shippingCreated.OrderId,
            shippingCreated.ShipmentReference);
    }
}
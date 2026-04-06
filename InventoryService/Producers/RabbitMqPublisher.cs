using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Shared.Contracts.Events;

namespace InventoryService.Producers;

public class RabbitMqPublisher
{
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(ILogger<RabbitMqPublisher> logger)
    {
        _logger = logger;
    }

    public async Task PublishInventoryConfirmed(InventoryConfirmed inventoryConfirmed)
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost"
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "inventory-confirmed",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var json = JsonSerializer.Serialize(inventoryConfirmed);
        var body = Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: "inventory-confirmed",
            body: body);

        _logger.LogInformation(
            "InventoryConfirmed published for OrderId: {OrderId}",
            inventoryConfirmed.OrderId);
    }
}
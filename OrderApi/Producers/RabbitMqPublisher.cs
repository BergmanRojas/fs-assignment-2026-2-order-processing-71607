using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Shared.Contracts.Events;

namespace OrderApi.Producers;

public class RabbitMqPublisher
{
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(ILogger<RabbitMqPublisher> logger)
    {
        _logger = logger;
    }

    public async Task PublishOrderSubmitted(OrderSubmitted orderSubmitted)
    {
        var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";

        var factory = new ConnectionFactory
        {
            HostName = rabbitMqHost
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: "order-submitted",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var json = JsonSerializer.Serialize(orderSubmitted);
        var body = Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: "order-submitted",
            body: body);

        _logger.LogInformation(
            "OrderSubmitted event published for OrderId: {OrderId}",
            orderSubmitted.OrderId);
    }
}
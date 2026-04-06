using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Shared.Contracts.Events;

namespace OrderApi.Producers;

public class RabbitMqPublisher
{
    public async Task PublishOrderSubmitted(OrderSubmitted orderSubmitted)
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost"
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

        Console.WriteLine($"[x] OrderSubmitted published: {json}");
    }
}
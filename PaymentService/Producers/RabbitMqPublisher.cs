using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Shared.Contracts.Events;

namespace PaymentService.Producers;

public class RabbitMqPublisher
{
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(ILogger<RabbitMqPublisher> logger)
    {
        _logger = logger;
    }

    public async Task PublishPaymentApproved(PaymentApproved paymentApproved)
    {
        var rabbitMqHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";

        var factory = new ConnectionFactory
        {
            HostName = rabbitMqHost
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.ExchangeDeclareAsync(
            exchange: "payment-approved-exchange",
            type: ExchangeType.Fanout,
            durable: false,
            autoDelete: false,
            arguments: null);

        var json = JsonSerializer.Serialize(paymentApproved);
        var body = Encoding.UTF8.GetBytes(json);

        await channel.BasicPublishAsync(
            exchange: "payment-approved-exchange",
            routingKey: string.Empty,
            body: body);

        _logger.LogInformation(
            "PaymentApproved published for OrderId: {OrderId}",
            paymentApproved.OrderId);
    }
}
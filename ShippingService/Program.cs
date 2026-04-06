using Serilog;
using ShippingService.Consumers;
using ShippingService.Producers;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/shippingservice-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog();
builder.Services.AddSingleton<RabbitMqPublisher>();
builder.Services.AddHostedService<PaymentApprovedConsumer>();

var host = builder.Build();
host.Run();
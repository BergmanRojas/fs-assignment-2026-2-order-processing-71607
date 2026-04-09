using PaymentService.Consumers;
using PaymentService.Producers;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/paymentservice-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog();
builder.Services.AddSingleton<RabbitMqPublisher>();
builder.Services.AddHostedService<InventoryConfirmedConsumer>();

var host = builder.Build();
host.Run();
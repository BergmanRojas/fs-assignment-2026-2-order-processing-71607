using InventoryService.Consumers;
using InventoryService.Producers;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/inventoryservice-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog();
builder.Services.AddSingleton<RabbitMqPublisher>();
builder.Services.AddHostedService<OrderSubmittedConsumer>();

var host = builder.Build();
host.Run();
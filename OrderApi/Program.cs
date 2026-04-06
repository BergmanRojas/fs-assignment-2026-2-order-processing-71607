using OrderApi.Consumers;
using OrderApi.Models;
using OrderApi.Producers;
using OrderApi.Services;
using Shared.Contracts.Events;
using Serilog;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/orderapi-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<RabbitMqPublisher>();
builder.Services.AddSingleton<OrderStore>();
builder.Services.AddHostedService<ShippingCreatedConsumer>();
builder.Host.UseSerilog();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new
{
    Name = "Order API",
    Status = "Running"
}));

app.MapGet("/api/orders", (OrderStore store) =>
{
    return Results.Ok(store.GetAll());
});

app.MapGet("/api/orders/{id:guid}", (Guid id, OrderStore store) =>
{
    var order = store.GetById(id);

    return order is null
        ? Results.NotFound(new { Message = "Order not found" })
        : Results.Ok(order);
});

app.MapGet("/api/orders/{id:guid}/status", (Guid id, OrderStore store) =>
{
    var order = store.GetById(id);

    return order is null
        ? Results.NotFound(new { Message = "Order not found" })
        : Results.Ok(new
        {
            order.OrderId,
            order.Status
        });
});

app.MapPost("/api/orders/checkout", async (RabbitMqPublisher publisher, OrderStore store, ILogger<Program> logger) =>
{
    var orderSubmitted = new OrderSubmitted
    {
        OrderId = Guid.NewGuid(),
        CustomerId = Guid.NewGuid(),
        SubmittedAt = DateTime.UtcNow
    };

    store.Add(new OrderRecord
    {
        OrderId = orderSubmitted.OrderId,
        CustomerId = orderSubmitted.CustomerId,
        Status = "Submitted",
        CreatedAt = orderSubmitted.SubmittedAt
    });

    logger.LogInformation("Order created with OrderId: {OrderId} for CustomerId: {CustomerId}",
        orderSubmitted.OrderId, orderSubmitted.CustomerId);

    await publisher.PublishOrderSubmitted(orderSubmitted);

    logger.LogInformation("OrderSubmitted event published for OrderId: {OrderId}", orderSubmitted.OrderId);

    return Results.Created($"/api/orders/{orderSubmitted.OrderId}", new
    {
        Message = "Order submitted successfully",
        orderSubmitted.OrderId,
        orderSubmitted.CustomerId,
        orderSubmitted.SubmittedAt,
        Status = "Submitted"
    });
});

app.MapPost("/api/orders/test-publish", async (RabbitMqPublisher publisher, OrderStore store, ILogger<Program> logger) =>
{
    var orderSubmitted = new OrderSubmitted
    {
        OrderId = Guid.NewGuid(),
        CustomerId = Guid.NewGuid(),
        SubmittedAt = DateTime.UtcNow
    };

    store.Add(new OrderRecord
    {
        OrderId = orderSubmitted.OrderId,
        CustomerId = orderSubmitted.CustomerId,
        Status = "Submitted",
        CreatedAt = orderSubmitted.SubmittedAt
    });

    logger.LogInformation("Test order created with OrderId: {OrderId}", orderSubmitted.OrderId);

    await publisher.PublishOrderSubmitted(orderSubmitted);

    logger.LogInformation("Test OrderSubmitted event published for OrderId: {OrderId}", orderSubmitted.OrderId);

    return Results.Ok(new
    {
        Message = "OrderSubmitted event published successfully",
        orderSubmitted.OrderId,
        orderSubmitted.CustomerId,
        orderSubmitted.SubmittedAt,
        Status = "Submitted"
    });
});

app.Run();
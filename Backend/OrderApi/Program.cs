using Microsoft.EntityFrameworkCore;
using OrderApi.Consumers;
using OrderApi.Data;
using OrderApi.Models;
using OrderApi.Producers;
using OrderApi.Services;
using Serilog;
using Shared.Contracts.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/orderapi-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5177",
                "http://localhost:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<RabbitMqPublisher>();


builder.Services.AddHostedService<InventoryConfirmedConsumer>();
builder.Services.AddHostedService<PaymentApprovedConsumer>();
builder.Services.AddHostedService<ShippingCreatedConsumer>();

builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<OrderDbInitializer>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<OrderDbInitializer>();
    dbInitializer.Initialize();
}

app.UseCors("Frontend");

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

app.MapGet("/api/orders/{id:guid}", async (Guid id, OrderDbContext db) =>
{
    var order = await db.Orders.FirstOrDefaultAsync(o => o.OrderId == id);

    return order is null
        ? Results.NotFound(new { Message = "Order not found" })
        : Results.Ok(order);
});

app.MapGet("/api/orders/{id:guid}/status", async (Guid id, OrderDbContext db) =>
{
    var order = await db.Orders.FirstOrDefaultAsync(o => o.OrderId == id);

    if (order is null)
        return Results.NotFound(new { Message = "Order not found" });

    return Results.Ok(new
    {
        order.OrderId,
        order.CustomerId,
        order.Status,
        order.InventoryConfirmedAt,
        order.PaymentApprovedAt,
        order.ShippingCreatedAt,
        order.ShipmentReference
    });
});

app.MapGet("/api/orders", async (string? status, OrderDbContext db) =>
{
    if (!string.IsNullOrWhiteSpace(status))
    {
        var filteredOrders = await db.Orders
            .Where(o => o.Status == status)
            .ToListAsync();

        return Results.Ok(filteredOrders);
    }

    var orders = await db.Orders.ToListAsync();
    return Results.Ok(orders);
});

app.MapGet("/api/customers/{id:guid}/orders", async (Guid id, OrderDbContext db) =>
{
    var orders = await db.Orders
        .Where(o => o.CustomerId == id)
        .ToListAsync();

    return Results.Ok(orders);
});

app.MapPost("/api/orders/checkout", async (RabbitMqPublisher publisher, OrderDbContext db, ILogger<Program> logger) =>
{
    var orderSubmitted = new OrderSubmitted
    {
        OrderId = Guid.NewGuid(),
        CustomerId = Guid.NewGuid(),
        SubmittedAt = DateTime.UtcNow
    };

    var order = new OrderRecord
    {
        Id = Guid.NewGuid(),
        OrderId = orderSubmitted.OrderId,
        CustomerId = orderSubmitted.CustomerId,
        Status = "Submitted",
        CreatedAt = orderSubmitted.SubmittedAt
    };

    db.Orders.Add(order);
    await db.SaveChangesAsync();

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

app.MapPost("/api/orders/test-publish", async (RabbitMqPublisher publisher, OrderDbContext db, ILogger<Program> logger) =>
{
    var orderSubmitted = new OrderSubmitted
    {
        OrderId = Guid.NewGuid(),
        CustomerId = Guid.NewGuid(),
        SubmittedAt = DateTime.UtcNow
    };

    var order = new OrderRecord
    {
        Id = Guid.NewGuid(),
        OrderId = orderSubmitted.OrderId,
        CustomerId = orderSubmitted.CustomerId,
        Status = "Submitted",
        CreatedAt = orderSubmitted.SubmittedAt
    };

    db.Orders.Add(order);
    await db.SaveChangesAsync();

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
app.MapGet("/api/products", async (OrderDbContext db) =>
{
    var products = await db.Products
        .OrderBy(p => p.Name)
        .ToListAsync();

    return Results.Ok(products);
});

app.Run();
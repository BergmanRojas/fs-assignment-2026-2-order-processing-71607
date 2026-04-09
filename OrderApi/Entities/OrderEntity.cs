namespace OrderApi.Entities;

public class OrderEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; } = "Submitted";
    public DateTime CreatedAt { get; set; }
    public DateTime? InventoryConfirmedAt { get; set; }
    public DateTime? PaymentApprovedAt { get; set; }
    public DateTime? ShippingCreatedAt { get; set; }
    public string? ShipmentReference { get; set; }
}
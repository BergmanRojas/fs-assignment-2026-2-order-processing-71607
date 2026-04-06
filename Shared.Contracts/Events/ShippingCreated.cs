namespace Shared.Contracts.Events;

public class ShippingCreated
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string ShipmentReference { get; set; } = string.Empty;
    public DateTime DispatchDate { get; set; }
}
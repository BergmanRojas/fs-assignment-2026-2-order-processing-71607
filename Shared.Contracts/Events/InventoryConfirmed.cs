namespace Shared.Contracts.Events;

public class InventoryConfirmed
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime ConfirmedAt { get; set; }
    public bool IsInStock { get; set; }
}
namespace Shared.Contracts.Events;

public class PaymentApproved
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime ApprovedAt { get; set; }
    public bool IsApproved { get; set; }
}
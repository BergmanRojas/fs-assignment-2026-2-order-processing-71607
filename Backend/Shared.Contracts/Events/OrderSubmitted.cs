namespace Shared.Contracts.Events;

public class OrderSubmitted
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime SubmittedAt { get; set; }
}
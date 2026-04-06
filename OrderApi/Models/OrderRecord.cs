namespace OrderApi.Models;

public class OrderRecord
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; } = "Submitted";
    public DateTime CreatedAt { get; set; }
}
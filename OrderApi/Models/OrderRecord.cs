using System;

namespace OrderApi.Models
{
    public class OrderRecord
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? InventoryConfirmedAt { get; set; }
        public DateTime? PaymentApprovedAt { get; set; }
        public DateTime? ShippingCreatedAt { get; set; }
        public string? ShipmentReference { get; set; }
    }
}
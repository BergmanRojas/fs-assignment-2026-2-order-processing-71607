using OrderApi.Models;

namespace OrderApi.Services;

public class OrderStore
{
    private readonly List<OrderRecord> _orders = new();

    public void Add(OrderRecord order)
    {
        _orders.Add(order);
    }

    public List<OrderRecord> GetAll()
    {
        return _orders;
    }

    public OrderRecord? GetById(Guid orderId)
    {
        return _orders.FirstOrDefault(o => o.OrderId == orderId);
    }

    public List<OrderRecord> GetByCustomerId(Guid customerId)
    {
        return _orders.Where(o => o.CustomerId == customerId).ToList();
    }

    public List<OrderRecord> GetByStatus(string status)
    {
        return _orders
            .Where(o => o.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public void UpdateStatus(Guid orderId, string status)
    {
        var order = GetById(orderId);
        if (order is not null)
        {
            order.Status = status;
        }
    }

    public void MarkInventoryConfirmed(Guid orderId, DateTime confirmedAt)
    {
        var order = GetById(orderId);
        if (order is not null)
        {
            order.Status = "InventoryConfirmed";
            order.InventoryConfirmedAt = confirmedAt;
        }
    }

    public void MarkPaymentApproved(Guid orderId, DateTime approvedAt)
    {
        var order = GetById(orderId);
        if (order is not null)
        {
            order.Status = "PaymentApproved";
            order.PaymentApprovedAt = approvedAt;
        }
    }

    public void MarkShippingCreated(Guid orderId, DateTime shippingCreatedAt, string shipmentReference)
    {
        var order = GetById(orderId);
        if (order is not null)
        {
            order.Status = "ShippingCreated";
            order.ShippingCreatedAt = shippingCreatedAt;
            order.ShipmentReference = shipmentReference;
        }
    }

    public void MarkCompleted(Guid orderId)
    {
        var order = GetById(orderId);
        if (order is not null)
        {
            order.Status = "Completed";
        }
    }

    public void MarkFailed(Guid orderId, string failedStatus)
    {
        var order = GetById(orderId);
        if (order is not null)
        {
            order.Status = failedStatus;
        }
    }
}
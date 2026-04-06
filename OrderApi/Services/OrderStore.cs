using OrderApi.Models;

namespace OrderApi.Services;

public class OrderStore
{
    private readonly List<OrderRecord> _orders = new();

    public List<OrderRecord> GetAll() => _orders;

    public OrderRecord? GetById(Guid orderId)
    {
        return _orders.FirstOrDefault(o => o.OrderId == orderId);
    }

    public void Add(OrderRecord order)
    {
        _orders.Add(order);
    }

    public void UpdateStatus(Guid orderId, string status)
    {
        var order = GetById(orderId);
        if (order is not null)
        {
            order.Status = status;
        }
    }
}
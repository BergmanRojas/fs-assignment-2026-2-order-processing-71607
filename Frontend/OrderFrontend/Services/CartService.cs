using OrderFrontend.Models;

namespace OrderFrontend.Services;

public class CartService
{
    private readonly List<CartItem> _items = new();

    public event Action? OnChange;

    public IReadOnlyList<CartItem> Items => _items;

    public int TotalItems => _items.Sum(x => x.Quantity);

    public decimal TotalAmount => _items.Sum(x => x.LineTotal);

    public void AddToCart(Product product)
    {
        var existingItem = _items.FirstOrDefault(x => x.ProductId == product.Id);

        if (existingItem is null)
        {
            _items.Add(new CartItem
            {
                ProductId = product.Id,
                Name = product.Name,
                Price = product.Price,
                Quantity = 1,
                ImageUrl = product.ImageUrl ?? string.Empty
            });
        }
        else
        {
            existingItem.Quantity++;
        }

        NotifyStateChanged();
    }

    public void IncreaseQuantity(Guid productId)
    {
        var item = _items.FirstOrDefault(x => x.ProductId == productId);
        if (item is null) return;

        item.Quantity++;
        NotifyStateChanged();
    }

    public void DecreaseQuantity(Guid productId)
    {
        var item = _items.FirstOrDefault(x => x.ProductId == productId);
        if (item is null) return;

        item.Quantity--;

        if (item.Quantity <= 0)
            _items.Remove(item);

        NotifyStateChanged();
    }

    public void RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(x => x.ProductId == productId);
        if (item is null) return;

        _items.Remove(item);
        NotifyStateChanged();
    }

    public void ClearCart()
    {
        _items.Clear();
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
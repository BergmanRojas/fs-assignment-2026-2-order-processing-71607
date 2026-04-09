using OrderApi.Entities;

namespace OrderApi.Data;

public static class SeedData
{
    public static void EnsureSeeded(OrderDbContext db)
    {
        if (!db.Products.Any())
        {
            var products = new List<Product>
            {
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Wireless Mouse",
                    Description = "Ergonomic wireless mouse for everyday productivity.",
                    Price = 24.99m,
                    StockQuantity = 25,
                    ImageUrl = BuildProductImage("Wireless Mouse", "#eff6ff", "#1d4ed8")
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Mechanical Keyboard",
                    Description = "Compact mechanical keyboard with tactile switches.",
                    Price = 79.99m,
                    StockQuantity = 15,
                    ImageUrl = BuildProductImage("Mechanical Keyboard", "#f5f3ff", "#6d28d9")
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "USB-C Hub",
                    Description = "Multiport USB-C hub with HDMI and USB 3.0 support.",
                    Price = 39.99m,
                    StockQuantity = 18,
                    ImageUrl = BuildProductImage("USB-C Hub", "#ecfeff", "#0f766e")
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Noise Cancelling Headphones",
                    Description = "Over-ear headphones with immersive sound and ANC.",
                    Price = 129.99m,
                    StockQuantity = 10,
                    ImageUrl = BuildProductImage("Headphones", "#fff7ed", "#c2410c")
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Laptop Stand",
                    Description = "Adjustable aluminium laptop stand for desk setup.",
                    Price = 34.99m,
                    StockQuantity = 20,
                    ImageUrl = BuildProductImage("Laptop Stand", "#f0fdf4", "#15803d")
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Webcam HD",
                    Description = "1080p webcam ideal for meetings and streaming.",
                    Price = 49.99m,
                    StockQuantity = 12,
                    ImageUrl = BuildProductImage("Webcam HD", "#fdf2f8", "#be185d")
                }
            };

            db.Products.AddRange(products);
            db.SaveChanges();
            return;
        }

        var existingProducts = db.Products.ToList();
        var hasChanges = false;

        foreach (var product in existingProducts)
        {
            if (string.IsNullOrWhiteSpace(product.ImageUrl) || product.ImageUrl.Contains("via.placeholder.com"))
            {
                product.ImageUrl = product.Name switch
                {
                    "Wireless Mouse" => BuildProductImage("Wireless Mouse", "#eff6ff", "#1d4ed8"),
                    "Mechanical Keyboard" => BuildProductImage("Mechanical Keyboard", "#f5f3ff", "#6d28d9"),
                    "USB-C Hub" => BuildProductImage("USB-C Hub", "#ecfeff", "#0f766e"),
                    "Noise Cancelling Headphones" => BuildProductImage("Headphones", "#fff7ed", "#c2410c"),
                    "Laptop Stand" => BuildProductImage("Laptop Stand", "#f0fdf4", "#15803d"),
                    "Webcam HD" => BuildProductImage("Webcam HD", "#fdf2f8", "#be185d"),
                    _ => BuildProductImage(product.Name, "#f8fafc", "#334155")
                };

                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            db.SaveChanges();
        }
    }

    private static string BuildProductImage(string title, string background, string accent)
    {
        var svg = $@"<svg xmlns='http://www.w3.org/2000/svg' width='420' height='280' viewBox='0 0 420 280'>
<rect width='420' height='280' rx='28' fill='{background}'/>
<rect x='28' y='28' width='364' height='224' rx='22' fill='white' stroke='{accent}' stroke-width='4'/>
<circle cx='82' cy='82' r='22' fill='{accent}' opacity='0.16'/>
<circle cx='338' cy='198' r='30' fill='{accent}' opacity='0.10'/>
<text x='210' y='124' text-anchor='middle' font-family='Arial, Helvetica, sans-serif' font-size='28' font-weight='700' fill='{accent}'>{title}</text>
<text x='210' y='165' text-anchor='middle' font-family='Arial, Helvetica, sans-serif' font-size='16' fill='#667085'>EverDeals Product</text>
</svg>";

        return "data:image/svg+xml;utf8," + Uri.EscapeDataString(svg);
    }
}
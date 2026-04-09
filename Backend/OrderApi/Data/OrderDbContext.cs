using Microsoft.EntityFrameworkCore;
using OrderApi.Models;
using OrderApi.Entities;

namespace OrderApi.Data;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    public DbSet<OrderRecord> Orders => Set<OrderRecord>();
    public DbSet<Product> Products => Set<Product>();
}
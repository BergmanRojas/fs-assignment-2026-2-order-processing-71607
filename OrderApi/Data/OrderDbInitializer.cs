using Microsoft.EntityFrameworkCore;

namespace OrderApi.Data;

public class OrderDbInitializer
{
    private readonly OrderDbContext _dbContext;

    public OrderDbInitializer(OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Initialize()
    {
        _dbContext.Database.Migrate();
        SeedData.EnsureSeeded(_dbContext);
    }
}
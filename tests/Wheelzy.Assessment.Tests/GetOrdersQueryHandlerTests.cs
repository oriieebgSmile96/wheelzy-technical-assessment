using Microsoft.EntityFrameworkCore;
using Wheelzy.Application.Orders.Queries.GetOrders;
using Wheelzy.Domain.Entities.Models;
using Wheelzy.Persistence;

namespace Wheelzy.Assessment.Tests;

public sealed class GetOrdersQueryHandlerTests
{
    [Fact]
    public async Task Handle_AppliesOnlyProvidedFilters()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);
        db.Orders.AddRange(
            new Order { Id = 1, OrderDate = new DateTime(2026, 1, 10), CustomerId = 1, StatusId = 1, IsActive = true, Total = 10 },
            new Order { Id = 2, OrderDate = new DateTime(2026, 2, 10), CustomerId = 2, StatusId = 2, IsActive = false, Total = 20 },
            new Order { Id = 3, OrderDate = new DateTime(2026, 3, 10), CustomerId = 1, StatusId = 2, IsActive = true, Total = 30 });
        await db.SaveChangesAsync();

        var handler = new GetOrdersQueryHandler(db);

        var unfiltered = await handler.Handle(new GetOrdersQuery { CustomerIds = [], StatusIds = [] }, CancellationToken.None);
        Assert.Equal(3, unfiltered.Count);

        var filtered = await handler.Handle(
            new GetOrdersQuery
            {
                DateFrom = new DateTime(2026, 2, 1),
                DateTo = new DateTime(2026, 12, 31),
                CustomerIds = [1, 2],
                StatusIds = [2],
                IsActive = true
            },
            CancellationToken.None);

        var row = Assert.Single(filtered);
        Assert.Equal(3, row.Id);
    }
}

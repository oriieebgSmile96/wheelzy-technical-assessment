using Microsoft.EntityFrameworkCore;
using Wheelzy.Application.Customers.Commands.UpdateCustomersBalanceByInvoices;
using Wheelzy.Domain.Entities.Models;
using Wheelzy.Persistence;

namespace Wheelzy.Assessment.Tests;

public sealed class UpdateCustomersBalanceByInvoicesCommandHandlerTests
{
    [Fact]
    public async Task Handle_LoadsCustomersOnceAndSumsTotals()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new ApplicationDbContext(options);
        db.Customers.AddRange(
            new Customer { Id = 1, Balance = 1000m },
            new Customer { Id = 2, Balance = 200m });
        await db.SaveChangesAsync();

        var handler = new UpdateCustomersBalanceByInvoicesCommandHandler(db);
        await handler.Handle(
            new UpdateCustomersBalanceByInvoicesCommand
            {
                Invoices =
                [
                    new Invoice { CustomerId = 1, Total = 100m },
                    new Invoice { CustomerId = 1, Total = 50m },
                    new Invoice { CustomerId = 2, Total = 25m },
                    new Invoice { CustomerId = null, Total = 999m }
                ]
            },
            CancellationToken.None);

        Assert.Equal(850m, db.Customers.Single(c => c.Id == 1).Balance);
        Assert.Equal(175m, db.Customers.Single(c => c.Id == 2).Balance);
    }
}

using Microsoft.EntityFrameworkCore;
using Wheelzy.Application.Cases.Queries.GetCaseSummaries;
using Wheelzy.Domain.Entities.Models;
using Wheelzy.Persistence;

namespace Wheelzy.Assessment.Tests;

public sealed class GetCaseSummariesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsOnlyCurrentBuyerQuoteAndStatus()
    {
        await using var db = CreateDatabase();
        var handler = new GetCaseSummariesQueryHandler(db);

        var rows = await handler.Handle(new GetCaseSummariesQuery(), CancellationToken.None);

        var row = Assert.Single(rows);
        Assert.Equal(2018, row.Year);
        Assert.Equal("Honda", row.Make);
        Assert.Equal("Civic", row.Model);
        Assert.Equal("EX", row.Submodel);
        Assert.Equal("Buyer ABC", row.CurrentBuyerName);
        Assert.Equal(500m, row.CurrentQuoteAmount);
        Assert.Equal("Accepted", row.CurrentStatusName);
        Assert.Null(row.CurrentStatusDate);
    }

    private static ApplicationDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new ApplicationDbContext(options);

        var make = new CarMake { Name = "Honda" };
        var model = new CarModel { Name = "Civic", Make = make };
        var submodel = new CarSubmodel { Name = "EX", Model = model };
        var zip = new ZipCode { Code = "32801" };
        var buyerAbc = new Buyer { Name = "Buyer ABC", QuoteAmount = 500m };
        var buyerXyz = new Buyer { Name = "Buyer XYZ", QuoteAmount = 700m };
        var accepted = new CaseStatusType { Name = "Accepted", RequiresStatusDate = false };
        var pending = new CaseStatusType { Name = "Pending Acceptance", RequiresStatusDate = false };

        var saleCase = new SaleCase
        {
            Year = 2018,
            Submodel = submodel,
            Zip = zip,
            ZipCode = zip.Code,
            CreatedOn = DateTime.UtcNow
        };

        saleCase.Quotes.Add(new CaseQuote { Buyer = buyerAbc, Amount = 500m, IsCurrent = true, CreatedOn = DateTime.UtcNow });
        saleCase.Quotes.Add(new CaseQuote { Buyer = buyerXyz, Amount = 700m, IsCurrent = false, CreatedOn = DateTime.UtcNow });
        saleCase.StatusHistory.Add(new CaseStatusHistory
        {
            StatusType = pending,
            IsCurrent = false,
            ChangedBy = "system",
            ChangedAt = DateTime.UtcNow.AddHours(-2)
        });
        saleCase.StatusHistory.Add(new CaseStatusHistory
        {
            StatusType = accepted,
            IsCurrent = true,
            ChangedBy = "orieb",
            ChangedAt = DateTime.UtcNow
        });

        db.Add(saleCase);
        db.SaveChanges();
        return db;
    }
}

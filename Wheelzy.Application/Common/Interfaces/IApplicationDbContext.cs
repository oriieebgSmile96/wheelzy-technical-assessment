using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Wheelzy.Domain.Entities.Models;

namespace Wheelzy.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<CarMake> Makes { get; }
    DbSet<CarModel> Models { get; }
    DbSet<CarSubmodel> Submodels { get; }
    DbSet<ZipCode> ZipCodes { get; }
    DbSet<Buyer> Buyers { get; }
    DbSet<BuyerZipCode> BuyerZipCodes { get; }
    DbSet<CaseStatusType> StatusTypes { get; }
    DbSet<SaleCase> Cases { get; }
    DbSet<CaseQuote> CaseQuotes { get; }
    DbSet<CaseStatusHistory> CaseStatusHistory { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Order> Orders { get; }

    DatabaseFacade Database { get; }

    int SaveChanges();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

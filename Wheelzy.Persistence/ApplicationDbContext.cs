using Microsoft.EntityFrameworkCore;
using Wheelzy.Application.Common.Interfaces;
using Wheelzy.Domain.Entities.Models;

namespace Wheelzy.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<CarMake> Makes => Set<CarMake>();
    public DbSet<CarModel> Models => Set<CarModel>();
    public DbSet<CarSubmodel> Submodels => Set<CarSubmodel>();
    public DbSet<ZipCode> ZipCodes => Set<ZipCode>();
    public DbSet<Buyer> Buyers => Set<Buyer>();
    public DbSet<BuyerZipCode> BuyerZipCodes => Set<BuyerZipCode>();
    public DbSet<CaseStatusType> StatusTypes => Set<CaseStatusType>();
    public DbSet<SaleCase> Cases => Set<SaleCase>();
    public DbSet<CaseQuote> CaseQuotes => Set<CaseQuote>();
    public DbSet<CaseStatusHistory> CaseStatusHistory => Set<CaseStatusHistory>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

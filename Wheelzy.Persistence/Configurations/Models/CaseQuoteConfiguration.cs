using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wheelzy.Domain.Entities.Models;

namespace Wheelzy.Persistence.Configurations.Models;

internal class CaseQuoteConfiguration : IEntityTypeConfiguration<CaseQuote>
{
    public void Configure(EntityTypeBuilder<CaseQuote> builder)
    {
        builder.ToTable("CaseQuote");
        builder.HasKey(x => x.CaseQuoteId);
        builder.Property(x => x.Amount).HasColumnType("decimal(12,2)");
        builder.Property(x => x.CreatedOn)
            .HasColumnName("CreatedAt")
            .HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.CreatedBy).HasMaxLength(256);
        builder.Property(x => x.ModifiedBy).HasMaxLength(256);
        builder.HasIndex(x => new { x.CaseId, x.BuyerId }).IsUnique();
        builder.HasIndex(x => x.CaseId)
            .IsUnique()
            .HasFilter("[IsCurrent] = 1")
            .HasDatabaseName("UX_CaseQuote_Current");
        builder.HasOne(x => x.Case)
            .WithMany(x => x.Quotes)
            .HasForeignKey(x => x.CaseId);
        builder.HasOne(x => x.Buyer)
            .WithMany(x => x.Quotes)
            .HasForeignKey(x => x.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

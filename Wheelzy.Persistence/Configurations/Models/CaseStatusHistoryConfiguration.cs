using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wheelzy.Domain.Entities.Models;

namespace Wheelzy.Persistence.Configurations.Models;

internal class CaseStatusHistoryConfiguration : IEntityTypeConfiguration<CaseStatusHistory>
{
    public void Configure(EntityTypeBuilder<CaseStatusHistory> builder)
    {
        builder.ToTable("CaseStatusHistory");
        builder.HasKey(x => x.CaseStatusId);
        builder.Property(x => x.ChangedBy).HasMaxLength(256).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(256);
        builder.Property(x => x.ModifiedBy).HasMaxLength(256);
        builder.HasIndex(x => x.CaseId)
            .IsUnique()
            .HasFilter("[IsCurrent] = 1")
            .HasDatabaseName("UX_CaseStatusHistory_Current");
        builder.HasOne(x => x.Case)
            .WithMany(x => x.StatusHistory)
            .HasForeignKey(x => x.CaseId);
        builder.HasOne(x => x.StatusType)
            .WithMany(x => x.History)
            .HasForeignKey(x => x.StatusTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

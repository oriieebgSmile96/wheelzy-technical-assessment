using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wheelzy.Domain.Entities.Models;

namespace Wheelzy.Persistence.Configurations.Models;

internal class SaleCaseConfiguration : IEntityTypeConfiguration<SaleCase>
{
    public void Configure(EntityTypeBuilder<SaleCase> builder)
    {
        builder.ToTable("SaleCase");
        builder.HasKey(x => x.CaseId);
        builder.Property(x => x.ZipCode).HasColumnType("char(5)").IsRequired();
        builder.Property(x => x.CreatedOn)
            .HasColumnName("CreatedAt")
            .HasDefaultValueSql("SYSUTCDATETIME()");
        builder.Property(x => x.CreatedBy).HasMaxLength(256);
        builder.Property(x => x.ModifiedBy).HasMaxLength(256);
        builder.HasOne(x => x.Submodel)
            .WithMany(x => x.Cases)
            .HasForeignKey(x => x.SubmodelId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Zip)
            .WithMany(x => x.Cases)
            .HasForeignKey(x => x.ZipCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

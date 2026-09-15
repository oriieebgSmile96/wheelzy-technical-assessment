using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wheelzy.Domain.Entities.Models;

namespace Wheelzy.Persistence.Configurations.Models;

internal class CaseStatusTypeConfiguration : IEntityTypeConfiguration<CaseStatusType>
{
    public void Configure(EntityTypeBuilder<CaseStatusType> builder)
    {
        builder.ToTable("CaseStatusType");
        builder.HasKey(x => x.StatusTypeId);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();
    }
}

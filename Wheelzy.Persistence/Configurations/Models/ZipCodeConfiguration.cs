using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wheelzy.Domain.Entities.Models;

namespace Wheelzy.Persistence.Configurations.Models;

internal class ZipCodeConfiguration : IEntityTypeConfiguration<ZipCode>
{
    public void Configure(EntityTypeBuilder<ZipCode> builder)
    {
        builder.ToTable("ZipCode");
        builder.HasKey(x => x.Code);
        builder.Property(x => x.Code)
            .HasColumnName("ZipCode")
            .HasColumnType("char(5)")
            .IsRequired();
    }
}

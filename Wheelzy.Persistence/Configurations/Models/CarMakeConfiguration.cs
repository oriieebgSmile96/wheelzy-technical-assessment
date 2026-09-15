using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wheelzy.Domain.Entities.Models;

namespace Wheelzy.Persistence.Configurations.Models;

internal class CarMakeConfiguration : IEntityTypeConfiguration<CarMake>
{
    public void Configure(EntityTypeBuilder<CarMake> builder)
    {
        builder.ToTable("CarMake");
        builder.HasKey(x => x.MakeId);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.Name).IsUnique();
    }
}

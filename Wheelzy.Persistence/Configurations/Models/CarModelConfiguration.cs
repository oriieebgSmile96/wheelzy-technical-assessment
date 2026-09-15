using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wheelzy.Domain.Entities.Models;

namespace Wheelzy.Persistence.Configurations.Models;

internal class CarModelConfiguration : IEntityTypeConfiguration<CarModel>
{
    public void Configure(EntityTypeBuilder<CarModel> builder)
    {
        builder.ToTable("CarModel");
        builder.HasKey(x => x.ModelId);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => new { x.MakeId, x.Name }).IsUnique();
        builder.HasOne(x => x.Make)
            .WithMany(x => x.Models)
            .HasForeignKey(x => x.MakeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

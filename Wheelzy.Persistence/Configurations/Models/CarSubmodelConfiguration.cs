using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wheelzy.Domain.Entities.Models;

namespace Wheelzy.Persistence.Configurations.Models;

internal class CarSubmodelConfiguration : IEntityTypeConfiguration<CarSubmodel>
{
    public void Configure(EntityTypeBuilder<CarSubmodel> builder)
    {
        builder.ToTable("CarSubmodel");
        builder.HasKey(x => x.SubmodelId);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => new { x.ModelId, x.Name }).IsUnique();
        builder.HasOne(x => x.Model)
            .WithMany(x => x.Submodels)
            .HasForeignKey(x => x.ModelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

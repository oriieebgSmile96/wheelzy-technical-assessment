using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wheelzy.Domain.Entities.Models;

namespace Wheelzy.Persistence.Configurations.Models;

internal class BuyerConfiguration : IEntityTypeConfiguration<Buyer>
{
    public void Configure(EntityTypeBuilder<Buyer> builder)
    {
        builder.ToTable("Buyer");
        builder.HasKey(x => x.BuyerId);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.QuoteAmount).HasColumnType("decimal(12,2)");
        builder.HasIndex(x => x.Name).IsUnique();
    }
}

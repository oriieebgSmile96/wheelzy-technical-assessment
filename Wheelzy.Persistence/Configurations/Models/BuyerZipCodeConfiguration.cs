using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wheelzy.Domain.Entities.Models;

namespace Wheelzy.Persistence.Configurations.Models;

internal class BuyerZipCodeConfiguration : IEntityTypeConfiguration<BuyerZipCode>
{
    public void Configure(EntityTypeBuilder<BuyerZipCode> builder)
    {
        builder.ToTable("BuyerZipCode");
        builder.HasKey(x => new { x.BuyerId, x.ZipCode });
        builder.Property(x => x.ZipCode).HasColumnType("char(5)");
        builder.HasOne(x => x.Buyer)
            .WithMany(x => x.ZipCodes)
            .HasForeignKey(x => x.BuyerId);
        builder.HasOne(x => x.Zip)
            .WithMany(x => x.BuyerCoverages)
            .HasForeignKey(x => x.ZipCode);
    }
}

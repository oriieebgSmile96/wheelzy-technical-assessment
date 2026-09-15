namespace Wheelzy.Domain.Entities.Models;

public class ZipCode
{
    public string Code { get; set; } = null!;
    public virtual ICollection<BuyerZipCode> BuyerCoverages { get; set; } = new List<BuyerZipCode>();
    public virtual ICollection<SaleCase> Cases { get; set; } = new List<SaleCase>();
}

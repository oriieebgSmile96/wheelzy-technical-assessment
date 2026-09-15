namespace Wheelzy.Domain.Entities.Models;

public class BuyerZipCode
{
    public int BuyerId { get; set; }
    public string ZipCode { get; set; } = null!;
    public virtual Buyer Buyer { get; set; } = null!;
    public virtual ZipCode Zip { get; set; } = null!;
}

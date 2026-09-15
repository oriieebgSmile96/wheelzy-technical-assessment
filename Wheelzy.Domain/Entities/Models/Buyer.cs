namespace Wheelzy.Domain.Entities.Models;

public class Buyer
{
    public int BuyerId { get; set; }
    public string Name { get; set; } = null!;
    public decimal QuoteAmount { get; set; }
    public virtual ICollection<BuyerZipCode> ZipCodes { get; set; } = new List<BuyerZipCode>();
    public virtual ICollection<CaseQuote> Quotes { get; set; } = new List<CaseQuote>();
}

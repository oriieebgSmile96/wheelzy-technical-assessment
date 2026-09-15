using Wheelzy.Domain.BaseModels;

namespace Wheelzy.Domain.Entities.Models;

public class CaseQuote : BaseEntity
{
    public int CaseQuoteId { get; set; }
    public int CaseId { get; set; }
    public int BuyerId { get; set; }
    public decimal Amount { get; set; }
    public bool IsCurrent { get; set; }
    public virtual SaleCase Case { get; set; } = null!;
    public virtual Buyer Buyer { get; set; } = null!;
}

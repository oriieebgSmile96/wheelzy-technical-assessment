using Wheelzy.Domain.BaseModels;

namespace Wheelzy.Domain.Entities.Models;

public class SaleCase : BaseEntity
{
    public int CaseId { get; set; }
    public short Year { get; set; }
    public int SubmodelId { get; set; }
    public string ZipCode { get; set; } = null!;
    public virtual CarSubmodel Submodel { get; set; } = null!;
    public virtual ZipCode Zip { get; set; } = null!;
    public virtual ICollection<CaseQuote> Quotes { get; set; } = new List<CaseQuote>();
    public virtual ICollection<CaseStatusHistory> StatusHistory { get; set; } = new List<CaseStatusHistory>();
}

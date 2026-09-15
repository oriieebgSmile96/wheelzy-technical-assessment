using Wheelzy.Domain.BaseModels;

namespace Wheelzy.Domain.Entities.Models;

public class CaseStatusHistory : BaseEntity
{
    public int CaseStatusId { get; set; }
    public int CaseId { get; set; }
    public int StatusTypeId { get; set; }
    public bool IsCurrent { get; set; }
    public string ChangedBy { get; set; } = null!;
    public DateTime ChangedAt { get; set; }
    public DateTime? StatusDate { get; set; }
    public virtual SaleCase Case { get; set; } = null!;
    public virtual CaseStatusType StatusType { get; set; } = null!;
}

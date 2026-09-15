namespace Wheelzy.Domain.Entities.Models;

public class CaseStatusType
{
    public int StatusTypeId { get; set; }
    public string Name { get; set; } = null!;
    public bool RequiresStatusDate { get; set; }
    public virtual ICollection<CaseStatusHistory> History { get; set; } = new List<CaseStatusHistory>();
}

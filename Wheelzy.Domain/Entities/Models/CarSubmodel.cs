namespace Wheelzy.Domain.Entities.Models;

public class CarSubmodel
{
    public int SubmodelId { get; set; }
    public int ModelId { get; set; }
    public string Name { get; set; } = null!;
    public virtual CarModel Model { get; set; } = null!;
    public virtual ICollection<SaleCase> Cases { get; set; } = new List<SaleCase>();
}

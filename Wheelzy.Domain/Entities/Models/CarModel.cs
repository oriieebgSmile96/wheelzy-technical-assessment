namespace Wheelzy.Domain.Entities.Models;

public class CarModel
{
    public int ModelId { get; set; }
    public int MakeId { get; set; }
    public string Name { get; set; } = null!;
    public virtual CarMake Make { get; set; } = null!;
    public virtual ICollection<CarSubmodel> Submodels { get; set; } = new List<CarSubmodel>();
}

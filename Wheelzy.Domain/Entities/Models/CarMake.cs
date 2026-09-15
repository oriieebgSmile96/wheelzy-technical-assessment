namespace Wheelzy.Domain.Entities.Models;

public class CarMake
{
    public int MakeId { get; set; }
    public string Name { get; set; } = null!;
    public virtual ICollection<CarModel> Models { get; set; } = new List<CarModel>();
}

namespace Wheelzy.Domain.Entities.Models;

public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public int CustomerId { get; set; }
    public int StatusId { get; set; }
    public bool IsActive { get; set; }
    public decimal Total { get; set; }
}

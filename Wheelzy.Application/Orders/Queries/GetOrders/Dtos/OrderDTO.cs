namespace Wheelzy.Application.Orders.Queries.GetOrders.Dtos;

public class OrderDTO
{
    public int Id { get; init; }
    public DateTime Date { get; init; }
    public int CustomerId { get; init; }
    public int StatusId { get; init; }
    public bool IsActive { get; init; }
    public decimal Total { get; init; }
}

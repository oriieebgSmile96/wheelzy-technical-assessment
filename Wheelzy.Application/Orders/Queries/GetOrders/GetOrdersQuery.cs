using MediatR;
using Wheelzy.Application.Orders.Queries.GetOrders.Dtos;

namespace Wheelzy.Application.Orders.Queries.GetOrders;

public class GetOrdersQuery : IRequest<List<OrderDTO>>
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public List<int>? CustomerIds { get; set; }
    public List<int>? StatusIds { get; set; }
    public bool? IsActive { get; set; }
}

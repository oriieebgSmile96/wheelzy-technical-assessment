using MediatR;
using Microsoft.EntityFrameworkCore;
using Wheelzy.Application.Common.Interfaces;
using Wheelzy.Application.Orders.Queries.GetOrders.Dtos;
using Wheelzy.Domain.Entities.Models;

namespace Wheelzy.Application.Orders.Queries.GetOrders;

public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, List<OrderDTO>>
{
    private readonly IApplicationDbContext _applicationDbContext;

    public GetOrdersQueryHandler(IApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public async Task<List<OrderDTO>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Order> query = _applicationDbContext.Orders.AsNoTracking();

        if (request.DateFrom.HasValue)
        {
            query = query.Where(order => order.OrderDate >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(order => order.OrderDate <= request.DateTo.Value);
        }

        if (request.CustomerIds is { Count: > 0 })
        {
            query = query.Where(order => request.CustomerIds.Contains(order.CustomerId));
        }

        if (request.StatusIds is { Count: > 0 })
        {
            query = query.Where(order => request.StatusIds.Contains(order.StatusId));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(order => order.IsActive == request.IsActive.Value);
        }

        return await query
            .Select(order => new OrderDTO
            {
                Id = order.Id,
                Date = order.OrderDate,
                CustomerId = order.CustomerId,
                StatusId = order.StatusId,
                IsActive = order.IsActive,
                Total = order.Total
            })
            .ToListAsync(cancellationToken);
    }
}

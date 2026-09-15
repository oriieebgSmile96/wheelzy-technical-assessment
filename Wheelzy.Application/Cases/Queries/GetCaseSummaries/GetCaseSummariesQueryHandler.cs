using MediatR;
using Microsoft.EntityFrameworkCore;
using Wheelzy.Application.Cases.Queries.GetCaseSummaries.Dtos;
using Wheelzy.Application.Common.Interfaces;

namespace Wheelzy.Application.Cases.Queries.GetCaseSummaries;

public class GetCaseSummariesQueryHandler : IRequestHandler<GetCaseSummariesQuery, List<CaseSummaryDto>>
{
    private readonly IApplicationDbContext _applicationDbContext;

    public GetCaseSummariesQueryHandler(IApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;
    }

    public Task<List<CaseSummaryDto>> Handle(GetCaseSummariesQuery request, CancellationToken cancellationToken)
    {
        return _applicationDbContext.Cases
            .AsNoTracking()
            .Select(c => new CaseSummaryDto
            {
                Year = c.Year,
                Make = c.Submodel.Model.Make.Name,
                Model = c.Submodel.Model.Name,
                Submodel = c.Submodel.Name,
                CurrentBuyerName = c.Quotes
                    .Where(q => q.IsCurrent)
                    .Select(q => q.Buyer.Name)
                    .FirstOrDefault(),
                CurrentQuoteAmount = c.Quotes
                    .Where(q => q.IsCurrent)
                    .Select(q => (decimal?)q.Amount)
                    .FirstOrDefault(),
                CurrentStatusName = c.StatusHistory
                    .Where(s => s.IsCurrent)
                    .Select(s => s.StatusType.Name)
                    .FirstOrDefault(),
                CurrentStatusDate = c.StatusHistory
                    .Where(s => s.IsCurrent)
                    .Select(s => s.StatusDate)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);
    }
}

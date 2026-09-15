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
        // One LEFT JOIN per "current" row (same shape as the SQL query) instead of
        // four correlated sub-queries (one per projected column).
        var query =
            from saleCase in _applicationDbContext.Cases.AsNoTracking()
            join currentQuote in _applicationDbContext.CaseQuotes.Where(q => q.IsCurrent)
                on saleCase.CaseId equals currentQuote.CaseId into quotes
            from currentQuote in quotes.DefaultIfEmpty()
            join currentStatus in _applicationDbContext.CaseStatusHistory.Where(s => s.IsCurrent)
                on saleCase.CaseId equals currentStatus.CaseId into statuses
            from currentStatus in statuses.DefaultIfEmpty()
            select new CaseSummaryDto
            {
                Year = saleCase.Year,
                Make = saleCase.Submodel.Model.Make.Name,
                Model = saleCase.Submodel.Model.Name,
                Submodel = saleCase.Submodel.Name,
                CurrentBuyerName = currentQuote == null ? null : currentQuote.Buyer.Name,
                CurrentQuoteAmount = currentQuote == null ? (decimal?)null : currentQuote.Amount,
                CurrentStatusName = currentStatus == null ? null : currentStatus.StatusType.Name,
                CurrentStatusDate = currentStatus == null ? (DateTime?)null : currentStatus.StatusDate
            };

        return query
            .ToListAsync(cancellationToken);
    }
}

using MediatR;
using Wheelzy.Application.Cases.Queries.GetCaseSummaries.Dtos;

namespace Wheelzy.Application.Cases.Queries.GetCaseSummaries;

public class GetCaseSummariesQuery : IRequest<List<CaseSummaryDto>>
{
}

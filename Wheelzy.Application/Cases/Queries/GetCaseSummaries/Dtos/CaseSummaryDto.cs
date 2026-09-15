namespace Wheelzy.Application.Cases.Queries.GetCaseSummaries.Dtos;

public class CaseSummaryDto
{
    public short Year { get; init; }
    public required string Make { get; init; }
    public required string Model { get; init; }
    public required string Submodel { get; init; }
    public string? CurrentBuyerName { get; init; }
    public decimal? CurrentQuoteAmount { get; init; }
    public string? CurrentStatusName { get; init; }
    public DateTime? CurrentStatusDate { get; init; }
}

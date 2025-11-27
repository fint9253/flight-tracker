using FlightTracker.Core.Interfaces;
using MediatR;

namespace FlightTracker.Api.Features.GetPriceHistory;

public record GetPriceHistoryQuery : IRequest<GetPriceHistoryResult>
{
    public Guid TrackedFlightId { get; init; }
    public int? Limit { get; init; }
}

public record GetPriceHistoryResult
{
    public bool FlightExists { get; init; }
    public List<PriceHistoryItem> History { get; init; } = new();
}

public record PriceHistoryItem
{
    public Guid Id { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime PollTimestamp { get; init; }
    public FlightOfferDetails? OfferDetails { get; init; }
}

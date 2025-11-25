using FlightTracker.Core.Interfaces;
using MediatR;

namespace FlightTracker.Api.Features.GetPriceHistory;

public class GetPriceHistoryHandler : IRequestHandler<GetPriceHistoryQuery, GetPriceHistoryResult>
{
    private readonly ITrackedFlightRepository _flightRepository;
    private readonly IPriceHistoryRepository _priceHistoryRepository;

    public GetPriceHistoryHandler(
        ITrackedFlightRepository flightRepository,
        IPriceHistoryRepository priceHistoryRepository)
    {
        _flightRepository = flightRepository;
        _priceHistoryRepository = priceHistoryRepository;
    }

    public async Task<GetPriceHistoryResult> Handle(
        GetPriceHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var flight = await _flightRepository.GetByIdAsync(request.TrackedFlightId, cancellationToken);

        if (flight == null)
        {
            return new GetPriceHistoryResult { FlightExists = false };
        }

        var history = request.Limit.HasValue
            ? await _priceHistoryRepository.GetByFlightIdAsync(request.TrackedFlightId, request.Limit.Value, cancellationToken)
            : await _priceHistoryRepository.GetByFlightIdAsync(request.TrackedFlightId, cancellationToken);

        return new GetPriceHistoryResult
        {
            FlightExists = true,
            History = history.Select(h => new PriceHistoryItem
            {
                Id = h.Id,
                Price = h.Price,
                Currency = h.Currency,
                PollTimestamp = h.PollTimestamp
            }).ToList()
        };
    }
}

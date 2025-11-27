using FlightTracker.Core.Interfaces;
using MediatR;

namespace FlightTracker.Api.Features.GetRecipientSummary;

public class GetRecipientSummaryHandler : IRequestHandler<GetRecipientSummaryQuery, RecipientSummaryResponse>
{
    private readonly INotificationRecipientRepository _repository;
    private readonly IPriceHistoryRepository _priceHistoryRepository;

    public GetRecipientSummaryHandler(
        INotificationRecipientRepository repository,
        IPriceHistoryRepository priceHistoryRepository)
    {
        _repository = repository;
        _priceHistoryRepository = priceHistoryRepository;
    }

    public async Task<RecipientSummaryResponse> Handle(
        GetRecipientSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var recipients = await _repository.GetAllWithFlightsAsync(cancellationToken);

        // Group recipients by email and get their unique flights
        var groupedRecipients = recipients
            .GroupBy(r => new { r.Email, r.Name })
            .Select(g => new
            {
                g.Key.Email,
                g.Key.Name,
                Flights = g.Select(r => r.TrackedFlight).Distinct().ToList()
            })
            .OrderBy(r => r.Email)
            .ToList();

        // Fetch price history for each flight and build response
        var result = new List<RecipientSummary>();

        foreach (var recipient in groupedRecipients)
        {
            var flightsWithPriceData = new List<RecipientFlight>();

            foreach (var flight in recipient.Flights)
            {
                // Get price history for this flight
                var priceHistory = await _priceHistoryRepository.GetByFlightIdAsync(
                    flight.Id, cancellationToken);

                var prices = priceHistory.OrderBy(p => p.PollTimestamp).ToList();
                var currentPrice = prices.LastOrDefault();
                var minPrice = prices.MinBy(p => p.Price);
                var maxPrice = prices.MaxBy(p => p.Price);
                var firstPrice = prices.FirstOrDefault();

                // Calculate price change percentage
                decimal? priceChangePercent = null;
                if (firstPrice != null && currentPrice != null && firstPrice.Price > 0)
                {
                    priceChangePercent = ((currentPrice.Price - firstPrice.Price) / firstPrice.Price) * 100;
                }

                flightsWithPriceData.Add(new RecipientFlight
                {
                    Id = flight.Id,
                    DepartureAirportIATA = flight.DepartureAirportIATA,
                    ArrivalAirportIATA = flight.ArrivalAirportIATA,
                    DepartureDate = flight.DepartureDate,
                    DateFlexibilityDays = flight.DateFlexibilityDays,
                    MaxStops = flight.MaxStops,
                    NotificationThresholdPercent = flight.NotificationThresholdPercent,
                    PollingIntervalHours = flight.PollingIntervalHours,
                    IsActive = flight.IsActive,
                    LastPolledAt = flight.LastPolledAt,
                    CreatedAt = flight.CreatedAt,
                    CurrentPrice = currentPrice?.Price,
                    MinPrice = minPrice?.Price,
                    MaxPrice = maxPrice?.Price,
                    PriceChangePercent = priceChangePercent,
                    Currency = currentPrice?.Currency,
                    PriceHistory = prices.Select(p => new PricePoint
                    {
                        Price = p.Price,
                        Currency = p.Currency,
                        PollTimestamp = p.PollTimestamp
                    }).ToList()
                });
            }

            result.Add(new RecipientSummary
            {
                Email = recipient.Email,
                Name = recipient.Name,
                TrackedFlights = flightsWithPriceData.OrderBy(f => f.DepartureDate).ToList()
            });
        }

        return new RecipientSummaryResponse
        {
            Recipients = result
        };
    }
}

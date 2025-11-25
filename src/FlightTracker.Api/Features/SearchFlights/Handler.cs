using FlightTracker.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlightTracker.Api.Features.SearchFlights;

public class SearchFlightsHandler : IRequestHandler<SearchFlightsQuery, List<FlightSearchResultDto>>
{
    private readonly IFlightSearchService _flightSearchService;
    private readonly ILogger<SearchFlightsHandler> _logger;

    public SearchFlightsHandler(
        IFlightSearchService flightSearchService,
        ILogger<SearchFlightsHandler> logger)
    {
        _flightSearchService = flightSearchService;
        _logger = logger;
    }

    public async Task<List<FlightSearchResultDto>> Handle(
        SearchFlightsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Searching flights: {Origin} -> {Destination}, {StartDate} to {EndDate}",
            request.OriginIATA, request.DestinationIATA, request.DepartureDateStart, request.DepartureDateEnd);

        var results = await _flightSearchService.SearchFlightsAsync(
            request.OriginIATA,
            request.DestinationIATA,
            request.DepartureDateStart,
            request.DepartureDateEnd,
            request.ReturnDateStart,
            request.ReturnDateEnd,
            cancellationToken);

        return results.Select(r => new FlightSearchResultDto
        {
            FlightNumber = r.FlightNumber,
            AirlineCode = r.AirlineCode,
            OriginIATA = r.OriginIATA,
            DestinationIATA = r.DestinationIATA,
            DepartureDate = r.DepartureDate,
            DepartureTime = r.DepartureTime.ToString("HH:mm"),
            ArrivalTime = r.ArrivalTime.ToString("HH:mm"),
            Price = r.Price,
            Currency = r.Currency,
            NumberOfStops = r.NumberOfStops,
            Duration = $"{(int)r.Duration.TotalHours}h {r.Duration.Minutes}m"
        }).ToList();
    }
}

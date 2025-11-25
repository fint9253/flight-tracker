using FlightTracker.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlightTracker.Api.Features.GetTrackedFlightsByRoute;

public class GetTrackedFlightsByRouteHandler : IRequestHandler<GetTrackedFlightsByRouteQuery, List<RouteGroupDto>>
{
    private readonly ITrackedFlightRepository _repository;
    private readonly ILogger<GetTrackedFlightsByRouteHandler> _logger;

    public GetTrackedFlightsByRouteHandler(
        ITrackedFlightRepository repository,
        ILogger<GetTrackedFlightsByRouteHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<RouteGroupDto>> Handle(
        GetTrackedFlightsByRouteQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting tracked flights by route for user {UserId}", request.UserId);

        var flights = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var groupedFlights = flights
            .GroupBy(f => new { f.DepartureAirportIATA, f.ArrivalAirportIATA })
            .Select(g =>
            {
                var flightDtos = g.Select(f => new RouteFlightDto
                {
                    Id = f.Id,
                    FlightNumber = f.FlightNumber,
                    DepartureDate = f.DepartureDate,
                    NotificationThresholdPercent = f.NotificationThresholdPercent,
                    IsActive = f.IsActive,
                    LastPolledAt = f.LastPolledAt
                })
                .OrderBy(f => f.DepartureDate)
                .ToList();

                var activeCount = flightDtos.Count(f => f.IsActive);
                var nextFlight = flightDtos.FirstOrDefault(f => f.DepartureDate >= today);

                return new RouteGroupDto
                {
                    Route = $"{g.Key.DepartureAirportIATA} → {g.Key.ArrivalAirportIATA}",
                    DepartureAirportIATA = g.Key.DepartureAirportIATA,
                    ArrivalAirportIATA = g.Key.ArrivalAirportIATA,
                    FlightCount = flightDtos.Count,
                    ActiveFlightCount = activeCount,
                    InactiveFlightCount = flightDtos.Count - activeCount,
                    EarliestDepartureDate = flightDtos.Min(f => f.DepartureDate),
                    LatestDepartureDate = flightDtos.Max(f => f.DepartureDate),
                    NextUpcomingFlight = nextFlight,
                    Flights = flightDtos
                };
            })
            .OrderBy(g => g.Route)
            .ToList();

        var totalFlights = flights.Count();
        _logger.LogInformation(
            "Found {FlightCount} tracked flights across {RouteCount} routes for user {UserId}",
            totalFlights, groupedFlights.Count, request.UserId);

        return groupedFlights;
    }
}

using MediatR;

namespace FlightTracker.Api.Features.GetTrackedFlightsByRoute;

public record GetTrackedFlightsByRouteQuery : IRequest<List<RouteGroupDto>>
{
    public string UserId { get; init; } = string.Empty;
}

public record RouteGroupDto
{
    public string Route { get; init; } = string.Empty;
    public string DepartureAirportIATA { get; init; } = string.Empty;
    public string ArrivalAirportIATA { get; init; } = string.Empty;
    public int FlightCount { get; init; }
    public List<RouteFlightDto> Flights { get; init; } = new();
}

public record RouteFlightDto
{
    public Guid Id { get; init; }
    public string FlightNumber { get; init; } = string.Empty;
    public DateOnly DepartureDate { get; init; }
    public decimal NotificationThresholdPercent { get; init; }
    public bool IsActive { get; init; }
    public DateTime? LastPolledAt { get; init; }
}

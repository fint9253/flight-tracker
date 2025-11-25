using FlightTracker.Core.Interfaces;
using MediatR;

namespace FlightTracker.Api.Features.SearchFlights;

public record SearchFlightsQuery : IRequest<List<FlightSearchResultDto>>
{
    public string OriginIATA { get; init; } = string.Empty;
    public string DestinationIATA { get; init; } = string.Empty;
    public DateOnly DepartureDateStart { get; init; }
    public DateOnly DepartureDateEnd { get; init; }
    public DateOnly? ReturnDateStart { get; init; }
    public DateOnly? ReturnDateEnd { get; init; }
}

public record FlightSearchResultDto
{
    public string FlightNumber { get; init; } = string.Empty;
    public string AirlineCode { get; init; } = string.Empty;
    public string OriginIATA { get; init; } = string.Empty;
    public string DestinationIATA { get; init; } = string.Empty;
    public DateOnly DepartureDate { get; init; }
    public string DepartureTime { get; init; } = string.Empty;
    public string ArrivalTime { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public int NumberOfStops { get; init; }
    public string Duration { get; init; } = string.Empty;
}

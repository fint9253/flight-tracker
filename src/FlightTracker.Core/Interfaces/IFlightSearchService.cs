namespace FlightTracker.Core.Interfaces;

public interface IFlightSearchService
{
    /// <summary>
    /// Searches for available flights on a route within a date range.
    /// </summary>
    /// <param name="originIATA">Origin airport IATA code (e.g., "DUB")</param>
    /// <param name="destinationIATA">Destination airport IATA code (e.g., "MAD")</param>
    /// <param name="departureDateStart">Start of departure date range</param>
    /// <param name="departureDateEnd">End of departure date range</param>
    /// <param name="returnDateStart">Optional start of return date range</param>
    /// <param name="returnDateEnd">Optional end of return date range</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of available flight offers</returns>
    Task<List<FlightSearchResult>> SearchFlightsAsync(
        string originIATA,
        string destinationIATA,
        DateOnly departureDateStart,
        DateOnly departureDateEnd,
        DateOnly? returnDateStart = null,
        DateOnly? returnDateEnd = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result from flight search containing flight details and pricing.
/// </summary>
public record FlightSearchResult
{
    public string FlightNumber { get; init; } = string.Empty;
    public string AirlineCode { get; init; } = string.Empty;
    public string OriginIATA { get; init; } = string.Empty;
    public string DestinationIATA { get; init; } = string.Empty;
    public DateOnly DepartureDate { get; init; }
    public TimeOnly DepartureTime { get; init; }
    public TimeOnly ArrivalTime { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public int NumberOfStops { get; init; }
    public TimeSpan Duration { get; init; }
}

namespace FlightTracker.Core.Interfaces;

public interface IFlightPriceService
{
    /// <summary>
    /// Retrieves the current price for a specific flight.
    /// </summary>
    /// <param name="flightNumber">Flight number (e.g., "AA123")</param>
    /// <param name="departureAirportIATA">Departure airport IATA code (e.g., "JFK")</param>
    /// <param name="arrivalAirportIATA">Arrival airport IATA code (e.g., "LAX")</param>
    /// <param name="departureDate">Date of departure</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Flight price data or null if not found</returns>
    Task<FlightPriceData?> GetFlightPriceAsync(
        string flightNumber,
        string departureAirportIATA,
        string arrivalAirportIATA,
        DateOnly departureDate,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Flight price data returned by the flight price service.
/// </summary>
public record FlightPriceData
{
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public DateTime RetrievedAt { get; init; } = DateTime.UtcNow;
    public string? CarrierCode { get; init; }
    public int NumberOfStops { get; init; }
}

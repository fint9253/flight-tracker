using FlightTracker.Core.Entities;
using FlightTracker.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlightTracker.Api.Features.BatchCreateTrackedFlights;

public class BatchCreateTrackedFlightsHandler : IRequestHandler<BatchCreateTrackedFlightsCommand, BatchCreateTrackedFlightsResult>
{
    private readonly ITrackedFlightRepository _repository;
    private readonly ILogger<BatchCreateTrackedFlightsHandler> _logger;

    public BatchCreateTrackedFlightsHandler(
        ITrackedFlightRepository repository,
        ILogger<BatchCreateTrackedFlightsHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<BatchCreateTrackedFlightsResult> Handle(
        BatchCreateTrackedFlightsCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing batch create for {Count} flights", request.Flights.Count);

        var results = new List<BatchFlightResult>();
        var successCount = 0;
        var failureCount = 0;

        for (int i = 0; i < request.Flights.Count; i++)
        {
            var flightRequest = request.Flights[i];

            try
            {
                var flight = new TrackedFlight
                {
                    UserId = flightRequest.UserId,
                    DepartureAirportIATA = flightRequest.DepartureAirportIATA,
                    ArrivalAirportIATA = flightRequest.ArrivalAirportIATA,
                    DepartureDate = flightRequest.DepartureDate,
                    DateFlexibilityDays = flightRequest.DateFlexibilityDays,
                    MaxStops = flightRequest.MaxStops,
                    NotificationThresholdPercent = flightRequest.NotificationThresholdPercent,
                    PollingIntervalMinutes = flightRequest.PollingIntervalMinutes,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var created = await _repository.AddAsync(flight, cancellationToken);

                results.Add(new BatchFlightResult
                {
                    Index = i,
                    Success = true,
                    FlightId = created.Id,
                    Route = $"{created.DepartureAirportIATA} → {created.ArrivalAirportIATA}"
                });

                successCount++;

                _logger.LogInformation(
                    "Batch item {Index}: Created tracked flight {FlightId} for user {UserId}",
                    i, created.Id, created.UserId);
            }
            catch (Exception ex)
            {
                results.Add(new BatchFlightResult
                {
                    Index = i,
                    Success = false,
                    Route = $"{flightRequest.DepartureAirportIATA} → {flightRequest.ArrivalAirportIATA}",
                    ErrorMessage = ex.Message
                });

                failureCount++;

                _logger.LogError(ex,
                    "Batch item {Index}: Failed to create tracked flight {Origin} → {Destination} for user {UserId}",
                    i, flightRequest.DepartureAirportIATA, flightRequest.ArrivalAirportIATA, flightRequest.UserId);
            }
        }

        _logger.LogInformation(
            "Batch create completed: {Success} succeeded, {Failure} failed out of {Total}",
            successCount, failureCount, request.Flights.Count);

        return new BatchCreateTrackedFlightsResult
        {
            TotalRequested = request.Flights.Count,
            SuccessCount = successCount,
            FailureCount = failureCount,
            Results = results
        };
    }
}

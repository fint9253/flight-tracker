using FlightTracker.Api.Models;
using FlightTracker.Core.Entities;
using FlightTracker.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlightTracker.Api.Controllers;

/// <summary>
/// API endpoints for managing tracked flights and price monitoring
/// </summary>
[ApiController]
[Route("api/tracking")]
[Produces("application/json")]
public class TrackedFlightsController : ControllerBase
{
    private readonly ITrackedFlightRepository _trackedFlightRepo;
    private readonly IPriceHistoryRepository _priceHistoryRepo;
    private readonly INotificationRecipientRepository _recipientRepo;
    private readonly ILogger<TrackedFlightsController> _logger;

    public TrackedFlightsController(
        ITrackedFlightRepository trackedFlightRepo,
        IPriceHistoryRepository priceHistoryRepo,
        INotificationRecipientRepository recipientRepo,
        ILogger<TrackedFlightsController> logger)
    {
        _trackedFlightRepo = trackedFlightRepo;
        _priceHistoryRepo = priceHistoryRepo;
        _recipientRepo = recipientRepo;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new tracked flight for price monitoring
    /// </summary>
    /// <param name="request">Flight tracking details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created tracked flight</returns>
    /// <response code="201">Flight tracking created successfully</response>
    /// <response code="400">Invalid request data</response>
    [HttpPost]
    [ProducesResponseType(typeof(TrackedFlightResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TrackedFlightResponse>> CreateTrackedFlight(
        [FromBody] CreateTrackedFlightRequest request,
        CancellationToken cancellationToken)
    {
        var flight = new TrackedFlight
        {
            UserId = request.UserId,
            FlightNumber = request.FlightNumber,
            DepartureAirportIATA = request.DepartureAirportIATA,
            ArrivalAirportIATA = request.ArrivalAirportIATA,
            DepartureDate = request.DepartureDate,
            NotificationThresholdPercent = request.NotificationThresholdPercent,
            PollingIntervalMinutes = request.PollingIntervalMinutes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _trackedFlightRepo.AddAsync(flight, cancellationToken);

        _logger.LogInformation(
            "Created tracked flight {FlightId} for user {UserId}: {FlightNumber} from {Origin} to {Destination}",
            created.Id, created.UserId, created.FlightNumber,
            created.DepartureAirportIATA, created.ArrivalAirportIATA);

        var response = MapToResponse(created);
        return CreatedAtAction(nameof(GetTrackedFlight), new { id = created.Id }, response);
    }

    /// <summary>
    /// Retrieves all tracked flights for a specific user
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of tracked flights</returns>
    /// <response code="200">Returns the list of tracked flights</response>
    /// <response code="400">User ID is required</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<TrackedFlightResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<TrackedFlightResponse>>> GetTrackedFlights(
        [FromQuery] string userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest("UserId is required");
        }

        var flights = await _trackedFlightRepo.GetByUserIdAsync(userId, cancellationToken);
        var responses = flights.Select(MapToResponse).ToList();

        return Ok(responses);
    }

    /// <summary>
    /// Retrieves a specific tracked flight by ID
    /// </summary>
    /// <param name="id">The tracked flight ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The tracked flight details</returns>
    /// <response code="200">Returns the tracked flight</response>
    /// <response code="404">Tracked flight not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TrackedFlightResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TrackedFlightResponse>> GetTrackedFlight(
        Guid id,
        CancellationToken cancellationToken)
    {
        var flight = await _trackedFlightRepo.GetByIdAsync(id, cancellationToken);

        if (flight == null)
        {
            return NotFound();
        }

        var response = MapToResponse(flight);
        return Ok(response);
    }

    /// <summary>
    /// Updates tracking settings for a flight (threshold, polling interval, active status)
    /// </summary>
    /// <param name="id">The tracked flight ID</param>
    /// <param name="request">Update request with optional fields</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated tracked flight</returns>
    /// <response code="200">Flight updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Tracked flight not found</response>
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(TrackedFlightResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TrackedFlightResponse>> UpdateTrackedFlight(
        Guid id,
        [FromBody] UpdateTrackedFlightRequest request,
        CancellationToken cancellationToken)
    {
        var flight = await _trackedFlightRepo.GetByIdAsync(id, cancellationToken);

        if (flight == null)
        {
            return NotFound();
        }

        // Update only provided fields
        if (request.NotificationThresholdPercent.HasValue)
        {
            flight.NotificationThresholdPercent = request.NotificationThresholdPercent.Value;
        }

        if (request.PollingIntervalMinutes.HasValue)
        {
            flight.PollingIntervalMinutes = request.PollingIntervalMinutes.Value;
        }

        if (request.IsActive.HasValue)
        {
            flight.IsActive = request.IsActive.Value;
        }

        await _trackedFlightRepo.UpdateAsync(flight, cancellationToken);

        _logger.LogInformation("Updated tracked flight {FlightId}", id);

        var response = MapToResponse(flight);
        return Ok(response);
    }

    /// <summary>
    /// Deletes a tracked flight and stops price monitoring
    /// </summary>
    /// <param name="id">The tracked flight ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Flight deleted successfully</response>
    /// <response code="404">Tracked flight not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTrackedFlight(
        Guid id,
        CancellationToken cancellationToken)
    {
        var flight = await _trackedFlightRepo.GetByIdAsync(id, cancellationToken);

        if (flight == null)
        {
            return NotFound();
        }

        await _trackedFlightRepo.DeleteAsync(id, cancellationToken);

        _logger.LogInformation(
            "Deleted tracked flight {FlightId} for user {UserId}",
            id, flight.UserId);

        return NoContent();
    }

    /// <summary>
    /// Retrieves price history for a tracked flight
    /// </summary>
    /// <param name="id">The tracked flight ID</param>
    /// <param name="limit">Optional limit on number of history records to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of historical prices</returns>
    /// <response code="200">Returns price history</response>
    /// <response code="404">Tracked flight not found</response>
    [HttpGet("{id}/history")]
    [ProducesResponseType(typeof(List<PriceHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PriceHistoryResponse>>> GetPriceHistory(
        Guid id,
        [FromQuery] int? limit,
        CancellationToken cancellationToken)
    {
        var flight = await _trackedFlightRepo.GetByIdAsync(id, cancellationToken);

        if (flight == null)
        {
            return NotFound();
        }

        var history = limit.HasValue
            ? await _priceHistoryRepo.GetByFlightIdAsync(id, limit.Value, cancellationToken)
            : await _priceHistoryRepo.GetByFlightIdAsync(id, cancellationToken);

        var responses = history.Select(h => new PriceHistoryResponse
        {
            Id = h.Id,
            Price = h.Price,
            Currency = h.Currency,
            PollTimestamp = h.PollTimestamp
        }).ToList();

        return Ok(responses);
    }

    /// <summary>
    /// Adds a notification recipient for price alerts
    /// </summary>
    /// <param name="id">The tracked flight ID</param>
    /// <param name="request">Recipient details (email and optional name)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created recipient</returns>
    /// <response code="201">Recipient added successfully</response>
    /// <response code="400">Invalid email address</response>
    /// <response code="404">Tracked flight not found</response>
    [HttpPost("{id}/recipients")]
    [ProducesResponseType(typeof(NotificationRecipientResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotificationRecipientResponse>> AddRecipient(
        Guid id,
        [FromBody] AddRecipientRequest request,
        CancellationToken cancellationToken)
    {
        var flight = await _trackedFlightRepo.GetByIdAsync(id, cancellationToken);

        if (flight == null)
        {
            return NotFound();
        }

        var recipient = new NotificationRecipient
        {
            TrackedFlightId = id,
            Email = request.Email,
            Name = request.Name,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _recipientRepo.AddAsync(recipient, cancellationToken);

        _logger.LogInformation(
            "Added recipient {Email} to tracked flight {FlightId}",
            created.Email, id);

        var response = new NotificationRecipientResponse
        {
            Id = created.Id,
            Email = created.Email,
            Name = created.Name,
            IsActive = created.IsActive,
            CreatedAt = created.CreatedAt
        };

        return CreatedAtAction(nameof(GetTrackedFlight), new { id }, response);
    }

    /// <summary>
    /// Removes a notification recipient from a tracked flight
    /// </summary>
    /// <param name="id">The tracked flight ID</param>
    /// <param name="recipientId">The recipient ID to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Recipient removed successfully</response>
    /// <response code="404">Tracked flight or recipient not found</response>
    [HttpDelete("{id}/recipients/{recipientId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveRecipient(
        Guid id,
        Guid recipientId,
        CancellationToken cancellationToken)
    {
        var flight = await _trackedFlightRepo.GetByIdAsync(id, cancellationToken);

        if (flight == null)
        {
            return NotFound("Tracked flight not found");
        }

        var recipient = await _recipientRepo.GetByIdAsync(recipientId, cancellationToken);

        if (recipient == null || recipient.TrackedFlightId != id)
        {
            return NotFound("Recipient not found");
        }

        await _recipientRepo.DeleteAsync(recipientId, cancellationToken);

        _logger.LogInformation(
            "Removed recipient {RecipientId} from tracked flight {FlightId}",
            recipientId, id);

        return NoContent();
    }

    private static TrackedFlightResponse MapToResponse(TrackedFlight flight)
    {
        return new TrackedFlightResponse
        {
            Id = flight.Id,
            UserId = flight.UserId,
            FlightNumber = flight.FlightNumber,
            DepartureAirportIATA = flight.DepartureAirportIATA,
            ArrivalAirportIATA = flight.ArrivalAirportIATA,
            DepartureDate = flight.DepartureDate,
            NotificationThresholdPercent = flight.NotificationThresholdPercent,
            PollingIntervalMinutes = flight.PollingIntervalMinutes,
            IsActive = flight.IsActive,
            LastPolledAt = flight.LastPolledAt,
            CreatedAt = flight.CreatedAt,
            UpdatedAt = flight.UpdatedAt,
            Recipients = flight.NotificationRecipients?.Select(r => new NotificationRecipientResponse
            {
                Id = r.Id,
                Email = r.Email,
                Name = r.Name,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt
            }).ToList() ?? new List<NotificationRecipientResponse>()
        };
    }
}

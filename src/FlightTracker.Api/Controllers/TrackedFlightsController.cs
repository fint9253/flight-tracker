using FlightTracker.Api.Models;
using FlightTracker.Core.Entities;
using FlightTracker.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FlightTracker.Api.Controllers;

[ApiController]
[Route("api/tracking")]
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

    // POST /api/tracking - Create tracked flight
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

    // GET /api/tracking - List user's tracked flights
    [HttpGet]
    [ProducesResponseType(typeof(List<TrackedFlightResponse>), StatusCodes.Status200OK)]
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

    // GET /api/tracking/{id} - Get specific flight
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

    // PATCH /api/tracking/{id} - Update settings
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

    // DELETE /api/tracking/{id} - Stop tracking
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

    // GET /api/tracking/{id}/history - Get price history
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

    // POST /api/tracking/{id}/recipients - Add notification recipient
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

    // DELETE /api/tracking/{id}/recipients/{recipientId} - Remove recipient
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

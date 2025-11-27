using System.Security.Claims;
using FlightTracker.Api.Features.AddRecipient;
using FlightTracker.Api.Features.BatchCreateTrackedFlights;
using FlightTracker.Api.Features.CreateTrackedFlight;
using FlightTracker.Api.Features.DeleteTrackedFlight;
using FlightTracker.Api.Features.GetPriceHistory;
using FlightTracker.Api.Features.GetTrackedFlight;
using FlightTracker.Api.Features.GetTrackedFlights;
using FlightTracker.Api.Features.GetTrackedFlightsByRoute;
using FlightTracker.Api.Features.RemoveRecipient;
using FlightTracker.Api.Features.SearchFlights;
using FlightTracker.Api.Features.UpdateTrackedFlight;
using FlightTracker.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightTracker.Api.Controllers;

/// <summary>
/// API endpoints for managing tracked flights and price monitoring
/// </summary>
[ApiController]
[Route("api/tracking")]
[Produces("application/json")]
[Authorize]
public class TrackedFlightsController : ControllerBase
{
    private readonly ISender _sender;

    public TrackedFlightsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Searches for available flights on a route within a date range
    /// </summary>
    /// <param name="request">Search criteria including origin, destination, and date ranges</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of available flights with prices</returns>
    /// <response code="200">Returns available flights</response>
    /// <response code="400">Invalid search criteria</response>
    [HttpPost("search")]
    [ProducesResponseType(typeof(List<FlightSearchResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<FlightSearchResultDto>>> SearchFlights(
        [FromBody] SearchFlightsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new SearchFlightsQuery
        {
            OriginIATA = request.OriginIATA,
            DestinationIATA = request.DestinationIATA,
            DepartureDateStart = request.DepartureDateStart,
            DepartureDateEnd = request.DepartureDateEnd,
            ReturnDateStart = request.ReturnDateStart,
            ReturnDateEnd = request.ReturnDateEnd
        };

        var results = await _sender.Send(query, cancellationToken);
        return Ok(results);
    }

    /// <summary>
    /// Creates multiple tracked flights in a single batch operation
    /// </summary>
    /// <param name="request">Batch request containing multiple flight tracking requests</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Batch result with success/failure status for each flight</returns>
    /// <response code="200">Returns batch result with per-item status</response>
    /// <response code="400">Invalid batch request</response>
    /// <response code="401">Unauthorized - authentication required</response>
    [HttpPost("batch")]
    [ProducesResponseType(typeof(BatchCreateTrackedFlightsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BatchCreateTrackedFlightsResponse>> BatchCreateTrackedFlights(
        [FromBody] BatchCreateTrackedFlightsRequest request,
        CancellationToken cancellationToken)
    {
        // Extract userId from authenticated JWT token
        var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }

        var command = new BatchCreateTrackedFlightsCommand
        {
            Flights = request.Flights.Select(f => new FlightToTrack
            {
                UserId = userId, // Use authenticated user ID for all flights
                DepartureAirportIATA = f.DepartureAirportIATA,
                ArrivalAirportIATA = f.ArrivalAirportIATA,
                DepartureDate = f.DepartureDate,
                DateFlexibilityDays = f.DateFlexibilityDays,
                MaxStops = f.MaxStops,
                NotificationThresholdPercent = f.NotificationThresholdPercent,
                PollingIntervalHours = f.PollingIntervalHours
            }).ToList()
        };

        var result = await _sender.Send(command, cancellationToken);

        var response = new BatchCreateTrackedFlightsResponse
        {
            TotalRequested = result.TotalRequested,
            SuccessCount = result.SuccessCount,
            FailureCount = result.FailureCount,
            Results = result.Results.Select(r => new BatchFlightItemResponse
            {
                Index = r.Index,
                Success = r.Success,
                FlightId = r.FlightId,
                Route = r.Route,
                ErrorMessage = r.ErrorMessage
            }).ToList()
        };

        return Ok(response);
    }

    /// <summary>
    /// Creates a new tracked flight for price monitoring
    /// </summary>
    /// <param name="request">Flight tracking details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created tracked flight</returns>
    /// <response code="201">Flight tracking created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized - authentication required</response>
    [HttpPost]
    [ProducesResponseType(typeof(TrackedFlightResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TrackedFlightResponse>> CreateTrackedFlight(
        [FromBody] CreateTrackedFlightRequest request,
        CancellationToken cancellationToken)
    {
        // Extract userId from authenticated JWT token (Clerk uses 'sub' claim)
        var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }

        var command = new CreateTrackedFlightCommand
        {
            UserId = userId,
            DepartureAirportIATA = request.DepartureAirportIATA,
            ArrivalAirportIATA = request.ArrivalAirportIATA,
            DepartureDate = request.DepartureDate,
            DateFlexibilityDays = request.DateFlexibilityDays,
            MaxStops = request.MaxStops,
            NotificationThresholdPercent = request.NotificationThresholdPercent,
            PollingIntervalHours = request.PollingIntervalHours
        };

        var result = await _sender.Send(command, cancellationToken);

        var response = new TrackedFlightResponse
        {
            Id = result.Id,
            UserId = result.UserId,
            DepartureAirportIATA = result.DepartureAirportIATA,
            ArrivalAirportIATA = result.ArrivalAirportIATA,
            DepartureDate = result.DepartureDate,
            DateFlexibilityDays = result.DateFlexibilityDays,
            MaxStops = result.MaxStops,
            NotificationThresholdPercent = result.NotificationThresholdPercent,
            PollingIntervalHours = result.PollingIntervalHours,
            IsActive = result.IsActive,
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.CreatedAt,
            Recipients = new List<NotificationRecipientResponse>()
        };

        return CreatedAtAction(nameof(GetTrackedFlight), new { id = result.Id }, response);
    }

    /// <summary>
    /// Retrieves all tracked flights for the authenticated user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of tracked flights</returns>
    /// <response code="200">Returns the list of tracked flights</response>
    /// <response code="401">Unauthorized - authentication required</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<TrackedFlightResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<TrackedFlightResponse>>> GetTrackedFlights(
        CancellationToken cancellationToken)
    {
        // Extract userId from authenticated JWT token
        var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }

        var query = new GetTrackedFlightsQuery { UserId = userId };
        var results = await _sender.Send(query, cancellationToken);

        var responses = results.Select(r => new TrackedFlightResponse
        {
            Id = r.Id,
            UserId = r.UserId,
            DepartureAirportIATA = r.DepartureAirportIATA,
            ArrivalAirportIATA = r.ArrivalAirportIATA,
            DepartureDate = r.DepartureDate,
            DateFlexibilityDays = r.DateFlexibilityDays,
            MaxStops = r.MaxStops,
            NotificationThresholdPercent = r.NotificationThresholdPercent,
            PollingIntervalHours = r.PollingIntervalHours,
            IsActive = r.IsActive,
            LastPolledAt = r.LastPolledAt,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
            Recipients = r.Recipients.Select(rec => new NotificationRecipientResponse
            {
                Id = rec.Id,
                Email = rec.Email,
                Name = rec.Name,
                IsActive = rec.IsActive,
                CreatedAt = rec.CreatedAt
            }).ToList()
        }).ToList();

        return Ok(responses);
    }

    /// <summary>
    /// Retrieves tracked flights grouped by route (origin to destination) for the authenticated user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Flights grouped by route with statistics</returns>
    /// <response code="200">Returns flights grouped by route</response>
    /// <response code="401">Unauthorized - authentication required</response>
    [HttpGet("by-route")]
    [ProducesResponseType(typeof(GetTrackedFlightsByRouteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GetTrackedFlightsByRouteResponse>> GetTrackedFlightsByRoute(
        CancellationToken cancellationToken)
    {
        // Extract userId from authenticated JWT token
        var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found in token");
        }

        var query = new GetTrackedFlightsByRouteQuery { UserId = userId };
        var routeGroups = await _sender.Send(query, cancellationToken);

        var response = new GetTrackedFlightsByRouteResponse
        {
            UserId = userId,
            TotalFlights = routeGroups.Sum(g => g.FlightCount),
            TotalRoutes = routeGroups.Count,
            Routes = routeGroups.Select(g => new RouteGroupResponse
            {
                Route = g.Route,
                DepartureAirportIATA = g.DepartureAirportIATA,
                ArrivalAirportIATA = g.ArrivalAirportIATA,
                FlightCount = g.FlightCount,
                ActiveFlightCount = g.ActiveFlightCount,
                InactiveFlightCount = g.InactiveFlightCount,
                EarliestDepartureDate = g.EarliestDepartureDate,
                LatestDepartureDate = g.LatestDepartureDate,
                NextUpcomingFlight = g.NextUpcomingFlight != null ? new RouteFlightResponse
                {
                    Id = g.NextUpcomingFlight.Id,
                    DepartureDate = g.NextUpcomingFlight.DepartureDate,
                    DateFlexibilityDays = g.NextUpcomingFlight.DateFlexibilityDays,
                    MaxStops = g.NextUpcomingFlight.MaxStops,
                    NotificationThresholdPercent = g.NextUpcomingFlight.NotificationThresholdPercent,
                    IsActive = g.NextUpcomingFlight.IsActive,
                    LastPolledAt = g.NextUpcomingFlight.LastPolledAt
                } : null,
                Flights = g.Flights.Select(f => new RouteFlightResponse
                {
                    Id = f.Id,
                    DepartureDate = f.DepartureDate,
                    DateFlexibilityDays = f.DateFlexibilityDays,
                    MaxStops = f.MaxStops,
                    NotificationThresholdPercent = f.NotificationThresholdPercent,
                    IsActive = f.IsActive,
                    LastPolledAt = f.LastPolledAt
                }).ToList()
            }).ToList()
        };

        return Ok(response);
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
        var query = new GetTrackedFlightQuery { Id = id };
        var result = await _sender.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound();
        }

        var response = new TrackedFlightResponse
        {
            Id = result.Id,
            UserId = result.UserId,
            DepartureAirportIATA = result.DepartureAirportIATA,
            ArrivalAirportIATA = result.ArrivalAirportIATA,
            DepartureDate = result.DepartureDate,
            DateFlexibilityDays = result.DateFlexibilityDays,
            MaxStops = result.MaxStops,
            NotificationThresholdPercent = result.NotificationThresholdPercent,
            PollingIntervalHours = result.PollingIntervalHours,
            IsActive = result.IsActive,
            LastPolledAt = result.LastPolledAt,
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.UpdatedAt,
            Recipients = result.Recipients.Select(r => new NotificationRecipientResponse
            {
                Id = r.Id,
                Email = r.Email,
                Name = r.Name,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt
            }).ToList()
        };

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
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TrackedFlightResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TrackedFlightResponse>> UpdateTrackedFlight(
        Guid id,
        [FromBody] UpdateTrackedFlightRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTrackedFlightCommand
        {
            Id = id,
            NotificationThresholdPercent = request.NotificationThresholdPercent,
            PollingIntervalHours = request.PollingIntervalHours,
            DateFlexibilityDays = request.DateFlexibilityDays,
            MaxStops = request.MaxStops,
            IsActive = request.IsActive
        };

        var result = await _sender.Send(command, cancellationToken);

        if (result == null)
        {
            return NotFound();
        }

        var response = new TrackedFlightResponse
        {
            Id = result.Id,
            UserId = result.UserId,
            DepartureAirportIATA = result.DepartureAirportIATA,
            ArrivalAirportIATA = result.ArrivalAirportIATA,
            DepartureDate = result.DepartureDate,
            DateFlexibilityDays = result.DateFlexibilityDays,
            MaxStops = result.MaxStops,
            NotificationThresholdPercent = result.NotificationThresholdPercent,
            PollingIntervalHours = result.PollingIntervalHours,
            IsActive = result.IsActive,
            LastPolledAt = result.LastPolledAt,
            CreatedAt = result.CreatedAt,
            UpdatedAt = result.UpdatedAt,
            Recipients = result.Recipients.Select(r => new NotificationRecipientResponse
            {
                Id = r.Id,
                Email = r.Email,
                Name = r.Name,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt
            }).ToList()
        };

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
        var command = new DeleteTrackedFlightCommand { Id = id };
        var success = await _sender.Send(command, cancellationToken);

        if (!success)
        {
            return NotFound();
        }

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
        var query = new GetPriceHistoryQuery
        {
            TrackedFlightId = id,
            Limit = limit
        };

        var result = await _sender.Send(query, cancellationToken);

        if (!result.FlightExists)
        {
            return NotFound();
        }

        var responses = result.History.Select(h => new PriceHistoryResponse
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
        var command = new AddRecipientCommand
        {
            TrackedFlightId = id,
            Email = request.Email,
            Name = request.Name
        };

        var result = await _sender.Send(command, cancellationToken);

        if (!result.FlightExists)
        {
            return NotFound();
        }

        var response = new NotificationRecipientResponse
        {
            Id = result.Recipient!.Id,
            Email = result.Recipient.Email,
            Name = result.Recipient.Name,
            IsActive = result.Recipient.IsActive,
            CreatedAt = result.Recipient.CreatedAt
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
        var command = new RemoveRecipientCommand
        {
            TrackedFlightId = id,
            RecipientId = recipientId
        };

        var result = await _sender.Send(command, cancellationToken);

        if (!result.Success)
        {
            return NotFound(result.ErrorMessage);
        }

        return NoContent();
    }
}

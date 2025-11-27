using FlightTracker.Api.Features.GetRecipientSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightTracker.Api.Controllers;

/// <summary>
/// API endpoints for managing notification recipients and their summaries
/// </summary>
[ApiController]
[Route("api/recipients")]
[Produces("application/json")]
[Authorize]
public class RecipientsController : ControllerBase
{
    private readonly ISender _sender;

    public RecipientsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Gets a summary of all recipients grouped by email with their tracked flights
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of recipients with their tracked flights</returns>
    /// <response code="200">Returns recipient summaries</response>
    /// <remarks>
    /// This endpoint allows anonymous access for n8n workflow integration.
    /// Only use on trusted local networks.
    /// </remarks>
    [HttpGet("summary")]
    [AllowAnonymous] // Allow n8n to access this endpoint without authentication
    [ProducesResponseType(typeof(RecipientSummaryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RecipientSummaryResponse>> GetRecipientSummary(
        CancellationToken cancellationToken)
    {
        var query = new GetRecipientSummaryQuery();
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }
}

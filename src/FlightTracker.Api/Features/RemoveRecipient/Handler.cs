using FlightTracker.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlightTracker.Api.Features.RemoveRecipient;

public class RemoveRecipientHandler : IRequestHandler<RemoveRecipientCommand, RemoveRecipientResult>
{
    private readonly ITrackedFlightRepository _flightRepository;
    private readonly INotificationRecipientRepository _recipientRepository;
    private readonly ILogger<RemoveRecipientHandler> _logger;

    public RemoveRecipientHandler(
        ITrackedFlightRepository flightRepository,
        INotificationRecipientRepository recipientRepository,
        ILogger<RemoveRecipientHandler> logger)
    {
        _flightRepository = flightRepository;
        _recipientRepository = recipientRepository;
        _logger = logger;
    }

    public async Task<RemoveRecipientResult> Handle(
        RemoveRecipientCommand request,
        CancellationToken cancellationToken)
    {
        var flight = await _flightRepository.GetByIdAsync(request.TrackedFlightId, cancellationToken);

        if (flight == null)
        {
            return new RemoveRecipientResult
            {
                Success = false,
                ErrorMessage = "Tracked flight not found"
            };
        }

        var recipient = await _recipientRepository.GetByIdAsync(request.RecipientId, cancellationToken);

        if (recipient == null || recipient.TrackedFlightId != request.TrackedFlightId)
        {
            return new RemoveRecipientResult
            {
                Success = false,
                ErrorMessage = "Recipient not found"
            };
        }

        await _recipientRepository.DeleteAsync(request.RecipientId, cancellationToken);

        _logger.LogInformation(
            "Removed recipient {RecipientId} from tracked flight {FlightId}",
            request.RecipientId, request.TrackedFlightId);

        return new RemoveRecipientResult { Success = true };
    }
}

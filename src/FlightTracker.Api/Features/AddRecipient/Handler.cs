using FlightTracker.Core.Entities;
using FlightTracker.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlightTracker.Api.Features.AddRecipient;

public class AddRecipientHandler : IRequestHandler<AddRecipientCommand, AddRecipientResult>
{
    private readonly ITrackedFlightRepository _flightRepository;
    private readonly INotificationRecipientRepository _recipientRepository;
    private readonly ILogger<AddRecipientHandler> _logger;

    public AddRecipientHandler(
        ITrackedFlightRepository flightRepository,
        INotificationRecipientRepository recipientRepository,
        ILogger<AddRecipientHandler> logger)
    {
        _flightRepository = flightRepository;
        _recipientRepository = recipientRepository;
        _logger = logger;
    }

    public async Task<AddRecipientResult> Handle(
        AddRecipientCommand request,
        CancellationToken cancellationToken)
    {
        var flight = await _flightRepository.GetByIdAsync(request.TrackedFlightId, cancellationToken);

        if (flight == null)
        {
            return new AddRecipientResult { FlightExists = false };
        }

        var recipient = new NotificationRecipient
        {
            TrackedFlightId = request.TrackedFlightId,
            Email = request.Email,
            Name = request.Name,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _recipientRepository.AddAsync(recipient, cancellationToken);

        _logger.LogInformation(
            "Added recipient {Email} to tracked flight {FlightId}",
            created.Email, request.TrackedFlightId);

        return new AddRecipientResult
        {
            FlightExists = true,
            Recipient = new RecipientData
            {
                Id = created.Id,
                Email = created.Email,
                Name = created.Name,
                IsActive = created.IsActive,
                CreatedAt = created.CreatedAt
            }
        };
    }
}

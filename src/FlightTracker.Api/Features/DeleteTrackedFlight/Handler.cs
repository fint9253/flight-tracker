using FlightTracker.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FlightTracker.Api.Features.DeleteTrackedFlight;

public class DeleteTrackedFlightHandler : IRequestHandler<DeleteTrackedFlightCommand, bool>
{
    private readonly ITrackedFlightRepository _repository;
    private readonly ILogger<DeleteTrackedFlightHandler> _logger;

    public DeleteTrackedFlightHandler(
        ITrackedFlightRepository repository,
        ILogger<DeleteTrackedFlightHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> Handle(
        DeleteTrackedFlightCommand request,
        CancellationToken cancellationToken)
    {
        var flight = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (flight == null)
        {
            return false;
        }

        await _repository.DeleteAsync(request.Id, cancellationToken);

        _logger.LogInformation(
            "Deleted tracked flight {FlightId} for user {UserId}",
            request.Id, flight.UserId);

        return true;
    }
}

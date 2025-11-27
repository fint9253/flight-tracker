using FluentValidation;

namespace FlightTracker.Api.Features.UpdateTrackedFlight;

public class UpdateTrackedFlightValidator : AbstractValidator<UpdateTrackedFlightCommand>
{
    public UpdateTrackedFlightValidator()
    {
        When(x => x.NotificationThresholdPercent.HasValue, () =>
        {
            RuleFor(x => x.NotificationThresholdPercent!.Value)
                .GreaterThan(0)
                .LessThanOrEqualTo(100)
                .WithMessage("Threshold must be between 0 and 100");
        });

        When(x => x.PollingIntervalHours.HasValue, () =>
        {
            RuleFor(x => x.PollingIntervalHours!.Value)
                .GreaterThanOrEqualTo(1)
                .LessThanOrEqualTo(24)
                .WithMessage("Polling interval must be between 1 and 24 hours");
        });
    }
}

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

        When(x => x.PollingIntervalMinutes.HasValue, () =>
        {
            RuleFor(x => x.PollingIntervalMinutes!.Value)
                .GreaterThanOrEqualTo(5)
                .LessThanOrEqualTo(1440)
                .WithMessage("Polling interval must be between 5 minutes and 24 hours");
        });
    }
}

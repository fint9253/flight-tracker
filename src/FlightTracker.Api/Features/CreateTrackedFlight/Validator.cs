using FluentValidation;

namespace FlightTracker.Api.Features.CreateTrackedFlight;

public class CreateTrackedFlightValidator : AbstractValidator<CreateTrackedFlightCommand>
{
    public CreateTrackedFlightValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.FlightNumber)
            .NotEmpty()
            .MaximumLength(20)
            .Matches(@"^[A-Z]{2}\d+$")
            .WithMessage("Flight number must be in format: 2 letters followed by numbers (e.g., AA123)");

        RuleFor(x => x.DepartureAirportIATA)
            .NotEmpty()
            .Length(3)
            .Matches(@"^[A-Z]{3}$")
            .WithMessage("Airport code must be 3 uppercase letters (e.g., JFK)");

        RuleFor(x => x.ArrivalAirportIATA)
            .NotEmpty()
            .Length(3)
            .Matches(@"^[A-Z]{3}$")
            .WithMessage("Airport code must be 3 uppercase letters (e.g., LAX)");

        RuleFor(x => x.DepartureDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Departure date must be today or in the future");

        RuleFor(x => x.NotificationThresholdPercent)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Threshold must be between 0 and 100");

        RuleFor(x => x.PollingIntervalMinutes)
            .GreaterThanOrEqualTo(5)
            .LessThanOrEqualTo(1440)
            .WithMessage("Polling interval must be between 5 minutes and 24 hours");
    }
}

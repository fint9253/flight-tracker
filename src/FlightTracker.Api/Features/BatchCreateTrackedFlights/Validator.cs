using FluentValidation;

namespace FlightTracker.Api.Features.BatchCreateTrackedFlights;

public class BatchCreateTrackedFlightsValidator : AbstractValidator<BatchCreateTrackedFlightsCommand>
{
    public BatchCreateTrackedFlightsValidator()
    {
        RuleFor(x => x.Flights)
            .NotEmpty()
            .WithMessage("At least one flight must be provided");

        RuleFor(x => x.Flights)
            .Must(flights => flights.Count <= 50)
            .WithMessage("Cannot track more than 50 flights in a single batch");

        RuleForEach(x => x.Flights).ChildRules(flight =>
        {
            flight.RuleFor(x => x.UserId)
                .NotEmpty()
                .MaximumLength(255);

            flight.RuleFor(x => x.DepartureAirportIATA)
                .NotEmpty()
                .Length(3)
                .Matches(@"^[A-Z]{3}$")
                .WithMessage("Airport code must be 3 uppercase letters (e.g., JFK)");

            flight.RuleFor(x => x.ArrivalAirportIATA)
                .NotEmpty()
                .Length(3)
                .Matches(@"^[A-Z]{3}$")
                .WithMessage("Airport code must be 3 uppercase letters (e.g., LAX)");

            flight.RuleFor(x => x.DepartureDate)
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Departure date must be today or in the future");

            flight.RuleFor(x => x.DateFlexibilityDays)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(7)
                .WithMessage("Date flexibility must be between 0 and 7 days");

            flight.RuleFor(x => x.MaxStops)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(3)
                .When(x => x.MaxStops.HasValue)
                .WithMessage("Max stops must be between 0 and 3");

            flight.RuleFor(x => x.NotificationThresholdPercent)
                .GreaterThan(0)
                .LessThanOrEqualTo(100)
                .WithMessage("Threshold must be between 0 and 100");

            flight.RuleFor(x => x.PollingIntervalHours)
                .GreaterThanOrEqualTo(1)
                .LessThanOrEqualTo(24)
                .WithMessage("Polling interval must be between 1 and 24 hours");
        });
    }
}

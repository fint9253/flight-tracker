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

            flight.RuleFor(x => x.FlightNumber)
                .NotEmpty()
                .MaximumLength(20)
                .Matches(@"^[A-Z]{2}\d+$")
                .WithMessage("Flight number must be in format: 2 letters followed by numbers (e.g., AA123)");

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

            flight.RuleFor(x => x.NotificationThresholdPercent)
                .GreaterThan(0)
                .LessThanOrEqualTo(100)
                .WithMessage("Threshold must be between 0 and 100");

            flight.RuleFor(x => x.PollingIntervalMinutes)
                .GreaterThanOrEqualTo(5)
                .LessThanOrEqualTo(1440)
                .WithMessage("Polling interval must be between 5 minutes and 24 hours");
        });
    }
}

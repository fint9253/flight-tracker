using FluentValidation;

namespace FlightTracker.Api.Features.SearchFlights;

public class SearchFlightsValidator : AbstractValidator<SearchFlightsQuery>
{
    public SearchFlightsValidator()
    {
        RuleFor(x => x.OriginIATA)
            .NotEmpty()
            .Length(3)
            .Matches(@"^[A-Z]{3}$")
            .WithMessage("Origin airport code must be 3 uppercase letters (e.g., DUB)");

        RuleFor(x => x.DestinationIATA)
            .NotEmpty()
            .Length(3)
            .Matches(@"^[A-Z]{3}$")
            .WithMessage("Destination airport code must be 3 uppercase letters (e.g., MAD)");

        RuleFor(x => x.DepartureDateStart)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Departure start date must be today or in the future");

        RuleFor(x => x.DepartureDateEnd)
            .GreaterThanOrEqualTo(x => x.DepartureDateStart)
            .WithMessage("Departure end date must be on or after start date");

        RuleFor(x => x.DepartureDateEnd)
            .LessThanOrEqualTo(x => x.DepartureDateStart.AddDays(30))
            .WithMessage("Date range cannot exceed 30 days");

        When(x => x.ReturnDateStart.HasValue, () =>
        {
            RuleFor(x => x.ReturnDateStart!.Value)
                .GreaterThanOrEqualTo(x => x.DepartureDateStart)
                .WithMessage("Return start date must be on or after departure start date");
        });

        When(x => x.ReturnDateEnd.HasValue && x.ReturnDateStart.HasValue, () =>
        {
            RuleFor(x => x.ReturnDateEnd!.Value)
                .GreaterThanOrEqualTo(x => x.ReturnDateStart!.Value)
                .WithMessage("Return end date must be on or after return start date");
        });
    }
}

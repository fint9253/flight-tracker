using FluentValidation;

namespace FlightTracker.Api.Features.GetTrackedFlights;

public class GetTrackedFlightsValidator : AbstractValidator<GetTrackedFlightsQuery>
{
    public GetTrackedFlightsValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");
    }
}

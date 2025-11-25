using FluentValidation;

namespace FlightTracker.Api.Features.AddRecipient;

public class AddRecipientValidator : AbstractValidator<AddRecipientCommand>
{
    public AddRecipientValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Valid email address is required");

        RuleFor(x => x.Name)
            .MaximumLength(255)
            .When(x => !string.IsNullOrEmpty(x.Name));
    }
}

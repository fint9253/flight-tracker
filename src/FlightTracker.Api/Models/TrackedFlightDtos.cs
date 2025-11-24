using FluentValidation;

namespace FlightTracker.Api.Models;

// Request DTOs
public record CreateTrackedFlightRequest
{
    public string UserId { get; init; } = string.Empty;
    public string FlightNumber { get; init; } = string.Empty;
    public string DepartureAirportIATA { get; init; } = string.Empty;
    public string ArrivalAirportIATA { get; init; } = string.Empty;
    public DateOnly DepartureDate { get; init; }
    public decimal NotificationThresholdPercent { get; init; } = 5.00m;
    public int PollingIntervalMinutes { get; init; } = 15;
}

public record UpdateTrackedFlightRequest
{
    public decimal? NotificationThresholdPercent { get; init; }
    public int? PollingIntervalMinutes { get; init; }
    public bool? IsActive { get; init; }
}

public record AddRecipientRequest
{
    public string Email { get; init; } = string.Empty;
    public string? Name { get; init; }
}

// Response DTOs
public record TrackedFlightResponse
{
    public Guid Id { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string FlightNumber { get; init; } = string.Empty;
    public string DepartureAirportIATA { get; init; } = string.Empty;
    public string ArrivalAirportIATA { get; init; } = string.Empty;
    public DateOnly DepartureDate { get; init; }
    public decimal NotificationThresholdPercent { get; init; }
    public int PollingIntervalMinutes { get; init; }
    public bool IsActive { get; init; }
    public DateTime? LastPolledAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public List<NotificationRecipientResponse> Recipients { get; init; } = new();
}

public record PriceHistoryResponse
{
    public Guid Id { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public DateTime PollTimestamp { get; init; }
}

public record NotificationRecipientResponse
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? Name { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}

// Validators
public class CreateTrackedFlightValidator : AbstractValidator<CreateTrackedFlightRequest>
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

public class UpdateTrackedFlightValidator : AbstractValidator<UpdateTrackedFlightRequest>
{
    public UpdateTrackedFlightValidator()
    {
        When(x => x.NotificationThresholdPercent.HasValue, () =>
        {
            RuleFor(x => x.NotificationThresholdPercent!.Value)
                .GreaterThan(0)
                .LessThanOrEqualTo(100);
        });

        When(x => x.PollingIntervalMinutes.HasValue, () =>
        {
            RuleFor(x => x.PollingIntervalMinutes!.Value)
                .GreaterThanOrEqualTo(5)
                .LessThanOrEqualTo(1440);
        });
    }
}

public class AddRecipientValidator : AbstractValidator<AddRecipientRequest>
{
    public AddRecipientValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);

        RuleFor(x => x.Name)
            .MaximumLength(100);
    }
}

using FluentValidation;

namespace FlightTracker.Api.Models;

// Request DTOs
public record SearchFlightsRequest
{
    public string OriginIATA { get; init; } = string.Empty;
    public string DestinationIATA { get; init; } = string.Empty;
    public DateOnly DepartureDateStart { get; init; }
    public DateOnly DepartureDateEnd { get; init; }
    public DateOnly? ReturnDateStart { get; init; }
    public DateOnly? ReturnDateEnd { get; init; }
}

public record CreateTrackedFlightRequest
{
    // UserId is extracted from JWT authentication, not from request body
    public string DepartureAirportIATA { get; init; } = string.Empty;
    public string ArrivalAirportIATA { get; init; } = string.Empty;
    public DateOnly DepartureDate { get; init; }
    public int DateFlexibilityDays { get; init; } = 3;
    public int? MaxStops { get; init; }
    public decimal NotificationThresholdPercent { get; init; } = 5.00m;
    public int PollingIntervalHours { get; init; } = 6;
}

public record UpdateTrackedFlightRequest
{
    public decimal? NotificationThresholdPercent { get; init; }
    public int? PollingIntervalHours { get; init; }
    public bool? IsActive { get; init; }
}

public record AddRecipientRequest
{
    public string Email { get; init; } = string.Empty;
    public string? Name { get; init; }
}

public record BatchCreateTrackedFlightsRequest
{
    public List<CreateTrackedFlightRequest> Flights { get; init; } = new();
}

public record GetTrackedFlightsByRouteRequest
{
    public string UserId { get; init; } = string.Empty;
}

// Response DTOs
public record TrackedFlightResponse
{
    public Guid Id { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string DepartureAirportIATA { get; init; } = string.Empty;
    public string ArrivalAirportIATA { get; init; } = string.Empty;
    public DateOnly DepartureDate { get; init; }
    public int DateFlexibilityDays { get; init; }
    public int? MaxStops { get; init; }
    public decimal NotificationThresholdPercent { get; init; }
    public int PollingIntervalHours { get; init; }
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

public record BatchCreateTrackedFlightsResponse
{
    public int TotalRequested { get; init; }
    public int SuccessCount { get; init; }
    public int FailureCount { get; init; }
    public List<BatchFlightItemResponse> Results { get; init; } = new();
}

public record BatchFlightItemResponse
{
    public int Index { get; init; }
    public bool Success { get; init; }
    public Guid? FlightId { get; init; }
    public string? Route { get; init; }
    public string? ErrorMessage { get; init; }
}

public record GetTrackedFlightsByRouteResponse
{
    public string UserId { get; init; } = string.Empty;
    public int TotalFlights { get; init; }
    public int TotalRoutes { get; init; }
    public List<RouteGroupResponse> Routes { get; init; } = new();
}

public record RouteGroupResponse
{
    public string Route { get; init; } = string.Empty;
    public string DepartureAirportIATA { get; init; } = string.Empty;
    public string ArrivalAirportIATA { get; init; } = string.Empty;
    public int FlightCount { get; init; }
    public int ActiveFlightCount { get; init; }
    public int InactiveFlightCount { get; init; }
    public DateOnly EarliestDepartureDate { get; init; }
    public DateOnly LatestDepartureDate { get; init; }
    public RouteFlightResponse? NextUpcomingFlight { get; init; }
    public List<RouteFlightResponse> Flights { get; init; } = new();
}

public record RouteFlightResponse
{
    public Guid Id { get; init; }
    public DateOnly DepartureDate { get; init; }
    public int DateFlexibilityDays { get; init; }
    public int? MaxStops { get; init; }
    public decimal NotificationThresholdPercent { get; init; }
    public bool IsActive { get; init; }
    public DateTime? LastPolledAt { get; init; }
}

// Validators for Request DTOs (HTTP layer validation)
// Note: MediatR Commands/Queries have their own validators in the Features folders
public class CreateTrackedFlightValidator : AbstractValidator<CreateTrackedFlightRequest>
{
    public CreateTrackedFlightValidator()
    {
        // UserId validation removed - comes from authenticated JWT claims

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

        RuleFor(x => x.DateFlexibilityDays)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(7)
            .WithMessage("Date flexibility must be between 0 and 7 days");

        RuleFor(x => x.MaxStops)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(3)
            .When(x => x.MaxStops.HasValue)
            .WithMessage("Max stops must be between 0 and 3");

        RuleFor(x => x.NotificationThresholdPercent)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Threshold must be between 0 and 100");

        RuleFor(x => x.PollingIntervalHours)
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

        When(x => x.PollingIntervalHours.HasValue, () =>
        {
            RuleFor(x => x.PollingIntervalHours!.Value)
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

public class SearchFlightsValidator : AbstractValidator<SearchFlightsRequest>
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

public class BatchCreateTrackedFlightsValidator : AbstractValidator<BatchCreateTrackedFlightsRequest>
{
    public BatchCreateTrackedFlightsValidator()
    {
        RuleFor(x => x.Flights)
            .NotEmpty()
            .WithMessage("At least one flight must be provided");

        RuleFor(x => x.Flights)
            .Must(flights => flights.Count <= 50)
            .WithMessage("Cannot track more than 50 flights in a single batch");

        RuleForEach(x => x.Flights).SetValidator(new CreateTrackedFlightValidator());
    }
}

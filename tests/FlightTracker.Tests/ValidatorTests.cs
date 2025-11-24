using FlightTracker.Api.Models;
using FluentAssertions;
using Xunit;

namespace FlightTracker.Tests;

public class CreateTrackedFlightValidatorTests
{
    private readonly CreateTrackedFlightValidator _validator = new();

    [Fact]
    public void ValidRequest_Passes()
    {
        // Arrange
        var request = new CreateTrackedFlightRequest
        {
            UserId = "user123",
            FlightNumber = "AA123",
            DepartureAirportIATA = "JFK",
            ArrivalAirportIATA = "LAX",
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            NotificationThresholdPercent = 5.00m,
            PollingIntervalMinutes = 15
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void UserId_Empty_Fails(string userId)
    {
        // Arrange
        var request = new CreateTrackedFlightRequest
        {
            UserId = userId,
            FlightNumber = "AA123",
            DepartureAirportIATA = "JFK",
            ArrivalAirportIATA = "LAX",
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateTrackedFlightRequest.UserId));
    }

    [Theory]
    [InlineData("AA123")]  // Valid
    [InlineData("BA456")]  // Valid
    [InlineData("DL1234")] // Valid
    public void FlightNumber_ValidFormat_Passes(string flightNumber)
    {
        // Arrange
        var request = new CreateTrackedFlightRequest
        {
            UserId = "user123",
            FlightNumber = flightNumber,
            DepartureAirportIATA = "JFK",
            ArrivalAirportIATA = "LAX",
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("AA")]      // Missing numbers
    [InlineData("123")]     // Missing letters
    [InlineData("aa123")]   // Lowercase letters
    [InlineData("A123")]    // Only one letter
    [InlineData("AAA123")]  // Three letters
    [InlineData("AA 123")]  // Space
    public void FlightNumber_InvalidFormat_Fails(string flightNumber)
    {
        // Arrange
        var request = new CreateTrackedFlightRequest
        {
            UserId = "user123",
            FlightNumber = flightNumber,
            DepartureAirportIATA = "JFK",
            ArrivalAirportIATA = "LAX",
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateTrackedFlightRequest.FlightNumber));
    }

    [Theory]
    [InlineData("JFK")]
    [InlineData("LAX")]
    [InlineData("ORD")]
    public void AirportIATA_ValidFormat_Passes(string iataCode)
    {
        // Arrange
        var request = new CreateTrackedFlightRequest
        {
            UserId = "user123",
            FlightNumber = "AA123",
            DepartureAirportIATA = iataCode,
            ArrivalAirportIATA = "LAX",
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("JF")]      // Too short
    [InlineData("JFKX")]    // Too long
    [InlineData("jfk")]     // Lowercase
    [InlineData("JF1")]     // Contains number
    [InlineData("JF ")]     // Contains space
    public void AirportIATA_InvalidFormat_Fails(string iataCode)
    {
        // Arrange
        var request = new CreateTrackedFlightRequest
        {
            UserId = "user123",
            FlightNumber = "AA123",
            DepartureAirportIATA = iataCode,
            ArrivalAirportIATA = "LAX",
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateTrackedFlightRequest.DepartureAirportIATA));
    }

    [Fact]
    public void DepartureDate_PastDate_Fails()
    {
        // Arrange
        var request = new CreateTrackedFlightRequest
        {
            UserId = "user123",
            FlightNumber = "AA123",
            DepartureAirportIATA = "JFK",
            ArrivalAirportIATA = "LAX",
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateTrackedFlightRequest.DepartureDate));
    }

    [Fact]
    public void DepartureDate_Today_Passes()
    {
        // Arrange
        var request = new CreateTrackedFlightRequest
        {
            UserId = "user123",
            FlightNumber = "AA123",
            DepartureAirportIATA = "JFK",
            ArrivalAirportIATA = "LAX",
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(101)]
    public void NotificationThresholdPercent_OutOfRange_Fails(decimal threshold)
    {
        // Arrange
        var request = new CreateTrackedFlightRequest
        {
            UserId = "user123",
            FlightNumber = "AA123",
            DepartureAirportIATA = "JFK",
            ArrivalAirportIATA = "LAX",
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            NotificationThresholdPercent = threshold
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateTrackedFlightRequest.NotificationThresholdPercent));
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(5)]
    [InlineData(50)]
    [InlineData(100)]
    public void NotificationThresholdPercent_InRange_Passes(decimal threshold)
    {
        // Arrange
        var request = new CreateTrackedFlightRequest
        {
            UserId = "user123",
            FlightNumber = "AA123",
            DepartureAirportIATA = "JFK",
            ArrivalAirportIATA = "LAX",
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            NotificationThresholdPercent = threshold
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(4)]     // Too low
    [InlineData(1441)]  // Too high
    public void PollingIntervalMinutes_OutOfRange_Fails(int interval)
    {
        // Arrange
        var request = new CreateTrackedFlightRequest
        {
            UserId = "user123",
            FlightNumber = "AA123",
            DepartureAirportIATA = "JFK",
            ArrivalAirportIATA = "LAX",
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            PollingIntervalMinutes = interval
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateTrackedFlightRequest.PollingIntervalMinutes));
    }

    [Theory]
    [InlineData(5)]     // Minimum
    [InlineData(15)]    // Default
    [InlineData(60)]    // 1 hour
    [InlineData(1440)]  // Maximum (24 hours)
    public void PollingIntervalMinutes_InRange_Passes(int interval)
    {
        // Arrange
        var request = new CreateTrackedFlightRequest
        {
            UserId = "user123",
            FlightNumber = "AA123",
            DepartureAirportIATA = "JFK",
            ArrivalAirportIATA = "LAX",
            DepartureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            PollingIntervalMinutes = interval
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}

public class AddRecipientValidatorTests
{
    private readonly AddRecipientValidator _validator = new();

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.user+tag@example.co.uk")]
    [InlineData("user123@test-domain.com")]
    public void Email_ValidFormat_Passes(string email)
    {
        // Arrange
        var request = new AddRecipientRequest
        {
            Email = email,
            Name = "Test User"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("notanemail")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    public void Email_InvalidFormat_Fails(string email)
    {
        // Arrange
        var request = new AddRecipientRequest
        {
            Email = email
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddRecipientRequest.Email));
    }

    [Fact]
    public void Name_Optional_Passes()
    {
        // Arrange
        var request = new AddRecipientRequest
        {
            Email = "user@example.com",
            Name = null
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Name_TooLong_Fails()
    {
        // Arrange
        var request = new AddRecipientRequest
        {
            Email = "user@example.com",
            Name = new string('a', 101) // 101 characters
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AddRecipientRequest.Name));
    }
}

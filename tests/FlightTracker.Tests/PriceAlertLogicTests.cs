using FluentAssertions;
using Xunit;

namespace FlightTracker.Tests;

public class PriceAlertLogicTests
{
    [Theory]
    [InlineData(500, 5, 475)]   // $500 avg, 5% threshold = $475
    [InlineData(1000, 10, 900)] // $1000 avg, 10% threshold = $900
    [InlineData(250, 20, 200)]  // $250 avg, 20% threshold = $200
    [InlineData(750, 15, 637.50)] // $750 avg, 15% threshold = $637.50
    public void CalculateThresholdPrice_ReturnsCorrectValue(
        decimal averagePrice,
        decimal thresholdPercent,
        decimal expectedThreshold)
    {
        // Arrange & Act
        var thresholdMultiplier = 1 - (thresholdPercent / 100);
        var thresholdPrice = averagePrice * thresholdMultiplier;

        // Assert
        thresholdPrice.Should().Be(expectedThreshold);
    }

    [Theory]
    [InlineData(500, 5, 450, true)]   // Current $450 < threshold $475 → Alert
    [InlineData(500, 5, 474.99, true)] // Current $474.99 < threshold $475 → Alert
    [InlineData(500, 5, 475, false)]  // Current $475 = threshold $475 → No alert
    [InlineData(500, 5, 480, false)]  // Current $480 > threshold $475 → No alert
    [InlineData(500, 5, 500, false)]  // Current $500 = average → No alert
    public void ShouldCreateAlert_GivenCurrentPrice_ReturnsCorrectly(
        decimal averagePrice,
        decimal thresholdPercent,
        decimal currentPrice,
        bool shouldAlert)
    {
        // Arrange
        var thresholdMultiplier = 1 - (thresholdPercent / 100);
        var thresholdPrice = averagePrice * thresholdMultiplier;

        // Act
        var result = currentPrice < thresholdPrice;

        // Assert
        result.Should().Be(shouldAlert);
    }

    [Theory]
    [InlineData(500, 450, -10)]      // 10% drop
    [InlineData(1000, 900, -10)]     // 10% drop
    [InlineData(250, 200, -20)]      // 20% drop
    [InlineData(500, 525, 5)]        // 5% increase
    [InlineData(100, 100, 0)]        // No change
    public void CalculatePercentageChange_ReturnsCorrectValue(
        decimal oldPrice,
        decimal newPrice,
        decimal expectedPercentage)
    {
        // Arrange & Act
        var percentageChange = ((newPrice - oldPrice) / oldPrice) * 100;

        // Assert
        percentageChange.Should().BeApproximately(expectedPercentage, 0.01m);
    }

    [Fact]
    public void PriceAlertScenario_Below5PercentThreshold_CreatesAlert()
    {
        // Arrange
        var priceHistory = new List<decimal> { 500, 510, 490, 505, 495 };
        var averagePrice = priceHistory.Average();
        var currentPrice = 470m;
        var thresholdPercent = 5m;

        // Act
        var thresholdMultiplier = 1 - (thresholdPercent / 100);
        var thresholdPrice = averagePrice * thresholdMultiplier;
        var shouldAlert = currentPrice < thresholdPrice;
        var percentageChange = ((currentPrice - averagePrice) / averagePrice) * 100;

        // Assert
        averagePrice.Should().Be(500m);
        thresholdPrice.Should().Be(475m);
        shouldAlert.Should().BeTrue();
        percentageChange.Should().BeApproximately(-6m, 0.01m);
    }

    [Fact]
    public void PriceAlertScenario_ExactlyAtThreshold_NoAlert()
    {
        // Arrange
        var priceHistory = new List<decimal> { 500, 500, 500, 500, 500 };
        var averagePrice = priceHistory.Average();
        var currentPrice = 475m; // Exactly at 5% threshold
        var thresholdPercent = 5m;

        // Act
        var thresholdMultiplier = 1 - (thresholdPercent / 100);
        var thresholdPrice = averagePrice * thresholdMultiplier;
        var shouldAlert = currentPrice < thresholdPrice;

        // Assert
        averagePrice.Should().Be(500m);
        thresholdPrice.Should().Be(475m);
        shouldAlert.Should().BeFalse(); // Not below, equal to threshold
    }

    [Fact]
    public void PriceAlertScenario_PriceIncrease_NoAlert()
    {
        // Arrange
        var priceHistory = new List<decimal> { 500, 510, 490, 505, 495 };
        var averagePrice = priceHistory.Average();
        var currentPrice = 550m; // Price increased
        var thresholdPercent = 5m;

        // Act
        var thresholdMultiplier = 1 - (thresholdPercent / 100);
        var thresholdPrice = averagePrice * thresholdMultiplier;
        var shouldAlert = currentPrice < thresholdPrice;

        // Assert
        averagePrice.Should().Be(500m);
        thresholdPrice.Should().Be(475m);
        shouldAlert.Should().BeFalse();
    }

    [Fact]
    public void PriceAlertScenario_HighVolatility_CorrectAverage()
    {
        // Arrange
        var priceHistory = new List<decimal> { 300, 700, 400, 600, 500 };
        var averagePrice = priceHistory.Average();
        var currentPrice = 450m;
        var thresholdPercent = 10m;

        // Act
        var thresholdMultiplier = 1 - (thresholdPercent / 100);
        var thresholdPrice = averagePrice * thresholdMultiplier;
        var shouldAlert = currentPrice < thresholdPrice;

        // Assert
        averagePrice.Should().Be(500m); // (300+700+400+600+500)/5 = 500
        thresholdPrice.Should().Be(450m); // 500 * 0.9 = 450
        shouldAlert.Should().BeFalse(); // 450 not < 450
    }

    [Theory]
    [InlineData(1, 1)]      // 1% threshold, only triggers on 1%+ drop
    [InlineData(25, 25)]    // 25% threshold, only triggers on 25%+ drop
    [InlineData(50, 50)]    // 50% threshold, only triggers on 50%+ drop
    public void DifferentThresholds_CalculateCorrectly(decimal thresholdPercent, decimal expectedDrop)
    {
        // Arrange
        var averagePrice = 100m;

        // Act
        var thresholdMultiplier = 1 - (thresholdPercent / 100);
        var thresholdPrice = averagePrice * thresholdMultiplier;
        var actualDrop = 100 - thresholdPrice;

        // Assert
        actualDrop.Should().Be(expectedDrop);
    }
}

using FluentAssertions;

namespace FoodDeliveryAppAPI.Tests.UnitTests.OrderTests;

public class DriverPaymentServiceTests
{
    private readonly DriverPaymentService _service;

    public DriverPaymentServiceTests()
    {
        _service = new DriverPaymentService();
    }

    [Fact]
    public void CalculatePayment_With1KmAnd1Minute_ReturnsCorrectAmount()
    {
        // Arrange
        var distanceInMeters = 2500.0;
        var durationInSeconds = 600.0;

        // Act
        var result = _service.CalculatePayment(distanceInMeters, durationInSeconds);

        // Assert
        result.Should().Be(575);
    }

    [Fact]
    public void CalculatePayment_WithRoundingScenario_HandlesCorrectly()
    {
        // Arrange
        var distanceInMeters = 2333.0;
        var durationInSeconds = 123.0;

        // Act
        var result = _service.CalculatePayment(distanceInMeters, durationInSeconds);

        // Assert
        // 300 (base) + 70 (2.333 km * 30 = 69.99 -> 70) + 41 (2.05 min * 20 = 41) = 411
        result.Should().Be(411);
    }
}

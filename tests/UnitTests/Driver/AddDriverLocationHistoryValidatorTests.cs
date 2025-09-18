using FluentAssertions;

namespace FoodDeliveryAppAPI.Tests.UnitTests.DriverTests;

public class AddDriverLocationHistoryValidatorTests
{
    private readonly AddDriverLocationHistoryValidator _validator;

    public AddDriverLocationHistoryValidatorTests()
    {
        _validator = new AddDriverLocationHistoryValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldReturnValidResult()
    {
        // Arrange
        var command = new AddDriverLocationHistoryCommand
        {
            DriverId = 1,
            Location = new Location { Latitude = 51.5074, Longitude = -0.1278 }, // London coordinates
            Accuracy = 5.0,
            Speed = 10.5,
            Heading = 90.0,
            DeliveryId = 100,
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(48.9, "Latitude must be between 49.0 and 59.0 degrees")] // Too low
    [InlineData(59.1, "Latitude must be between 49.0 and 59.0 degrees")] // Too high
    public void Validate_WithInvalidLatitude_ShouldReturnError(
        double latitude,
        string expectedError
    )
    {
        // Arrange
        var command = new AddDriverLocationHistoryCommand
        {
            DriverId = 1,
            Location = new Location { Latitude = latitude, Longitude = -0.1278 },
            Accuracy = 5.0,
            Speed = 10.5,
            Heading = 90.0,
            DeliveryId = 100,
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(expectedError);
    }

    [Theory]
    [InlineData(-8.1, "Longitude must be between -8.0 and 2.0 degrees")] // Too low
    [InlineData(2.1, "Longitude must be between -8.0 and 2.0 degrees")] // Too high
    public void Validate_WithInvalidLongitude_ShouldReturnError(
        double longitude,
        string expectedError
    )
    {
        // Arrange
        var command = new AddDriverLocationHistoryCommand
        {
            DriverId = 1,
            Location = new Location { Latitude = 51.5074, Longitude = longitude },
            Accuracy = 5.0,
            Speed = 10.5,
            Heading = 90.0,
            DeliveryId = 100,
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(expectedError);
    }

    [Theory]
    [InlineData(0.0, "Accuracy must be greater than 0")]
    [InlineData(-1.0, "Accuracy must be greater than 0")]
    [InlineData(101.0, "Accuracy must be less than 100 meters")]
    public void Validate_WithInvalidAccuracy_ShouldReturnError(
        double accuracy,
        string expectedError
    )
    {
        // Arrange
        var command = new AddDriverLocationHistoryCommand
        {
            DriverId = 1,
            Location = new Location { Latitude = 51.5074, Longitude = -0.1278 },
            Accuracy = accuracy,
            Speed = 10.5,
            Heading = 90.0,
            DeliveryId = 100,
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(expectedError);
    }

    [Theory]
    [InlineData(-1.0, "Speed cannot be negative")]
    [InlineData(55.57, "Speed must be less than 200 km/h")] // Just over 200 km/h
    public void Validate_WithInvalidSpeed_ShouldReturnError(double speed, string expectedError)
    {
        // Arrange
        var command = new AddDriverLocationHistoryCommand
        {
            DriverId = 1,
            Location = new Location { Latitude = 51.5074, Longitude = -0.1278 },
            Accuracy = 5.0,
            Speed = speed,
            Heading = 90.0,
            DeliveryId = 100,
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(expectedError);
    }

    [Theory]
    [InlineData(-1.0, "Heading must be between 0 and 359.99 degrees")]
    [InlineData(360.0, "Heading must be between 0 and 359.99 degrees")]
    [InlineData(360.1, "Heading must be between 0 and 359.99 degrees")]
    public void Validate_WithInvalidHeading_ShouldReturnError(double heading, string expectedError)
    {
        // Arrange
        var command = new AddDriverLocationHistoryCommand
        {
            DriverId = 1,
            Location = new Location { Latitude = 51.5074, Longitude = -0.1278 },
            Accuracy = 5.0,
            Speed = 10.5,
            Heading = heading,
            DeliveryId = 100,
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(expectedError);
    }

    [Fact]
    public void Validate_WithMultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var command = new AddDriverLocationHistoryCommand
        {
            DriverId = 1,
            Location = new Location { Latitude = 60.0, Longitude = 3.0 }, // Both out of range
            Accuracy = -1.0,
            Speed = -5.0,
            Heading = 400.0,
            DeliveryId = 100,
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(5);
        result.Errors.Should().Contain("Latitude must be between 49.0 and 59.0 degrees");
        result.Errors.Should().Contain("Longitude must be between -8.0 and 2.0 degrees");
        result.Errors.Should().Contain("Accuracy must be greater than 0");
        result.Errors.Should().Contain("Speed cannot be negative");
        result.Errors.Should().Contain("Heading must be between 0 and 359.99 degrees");
    }

    [Theory]
    [InlineData(49.0, -8.0)] // Minimum valid values
    [InlineData(59.0, 2.0)] // Maximum valid values
    [InlineData(51.5074, -0.1278)] // London
    [InlineData(55.9533, -3.1883)] // Edinburgh
    public void Validate_WithValidLocationBoundaries_ShouldReturnValid(
        double latitude,
        double longitude
    )
    {
        // Arrange
        var command = new AddDriverLocationHistoryCommand
        {
            DriverId = 1,
            Location = new Location { Latitude = latitude, Longitude = longitude },
            Accuracy = 5.0,
            Speed = 10.5,
            Heading = 90.0,
            DeliveryId = 100,
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0.1)] // Minimum valid accuracy
    [InlineData(100.0)] // Maximum valid accuracy
    [InlineData(5.0)] // Typical accuracy
    public void Validate_WithValidAccuracy_ShouldReturnValid(double accuracy)
    {
        // Arrange
        var command = new AddDriverLocationHistoryCommand
        {
            DriverId = 1,
            Location = new Location { Latitude = 51.5074, Longitude = -0.1278 },
            Accuracy = accuracy,
            Speed = 10.5,
            Heading = 90.0,
            DeliveryId = 100,
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0.0)] // Stopped
    [InlineData(55.56)] // Exactly 200 km/h
    [InlineData(13.89)] // 50 km/h in m/s
    public void Validate_WithValidSpeed_ShouldReturnValid(double speed)
    {
        // Arrange
        var command = new AddDriverLocationHistoryCommand
        {
            DriverId = 1,
            Location = new Location { Latitude = 51.5074, Longitude = -0.1278 },
            Accuracy = 5.0,
            Speed = speed,
            Heading = 90.0,
            DeliveryId = 100,
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0.0)] // North
    [InlineData(90.0)] // East
    [InlineData(180.0)] // South
    [InlineData(270.0)] // West
    [InlineData(359.99)] // Just under full circle
    public void Validate_WithValidHeading_ShouldReturnValid(double heading)
    {
        // Arrange
        var command = new AddDriverLocationHistoryCommand
        {
            DriverId = 1,
            Location = new Location { Latitude = 51.5074, Longitude = -0.1278 },
            Accuracy = 5.0,
            Speed = 10.5,
            Heading = heading,
            DeliveryId = 100,
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}

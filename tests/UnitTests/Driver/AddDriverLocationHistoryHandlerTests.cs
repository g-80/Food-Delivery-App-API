using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FoodDeliveryAppAPI.Tests.UnitTests.DriverTests;

public class AddDriverLocationHistoryHandlerTests
{
    private readonly Mock<IDriverRepository> _mockDriverRepository;
    private readonly Mock<ILogger<AddDriverLocationHistoryHandler>> _mockLogger;
    private readonly AddDriverLocationHistoryHandler _handler;

    public AddDriverLocationHistoryHandlerTests()
    {
        _mockDriverRepository = new Mock<IDriverRepository>();
        _mockLogger = new Mock<ILogger<AddDriverLocationHistoryHandler>>();
        _handler = new AddDriverLocationHistoryHandler(
            _mockDriverRepository.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldAddLocationHistory()
    {
        // Arrange
        var command = new AddDriverLocationHistoryCommand
        {
            DriverId = 1,
            Location = new Location { Latitude = 51.5074, Longitude = -0.1278 },
            Accuracy = 5.0,
            Speed = 10.5,
            Heading = 90.0,
            DeliveryId = 100,
        };

        _mockDriverRepository
            .Setup(x => x.AddDriverLocationHistoryAsync(It.IsAny<DriverLocationHistory>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command);

        // Assert
        _mockDriverRepository.Verify(
            x =>
                x.AddDriverLocationHistoryAsync(
                    It.Is<DriverLocationHistory>(h =>
                        h.DriverId == command.DriverId
                        && h.Location.Latitude == command.Location.Latitude
                        && h.Location.Longitude == command.Location.Longitude
                        && h.Accuracy == command.Accuracy
                        && h.Speed == command.Speed
                        && h.Heading == command.Heading
                        && h.DeliveryId == command.DeliveryId
                        && h.Timestamp.Date == DateTime.UtcNow.Date
                    )
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var command = new AddDriverLocationHistoryCommand
        {
            DriverId = 1,
            Location = new Location { Latitude = 51.5074, Longitude = -0.1278 },
            Accuracy = 5.0,
            Speed = 10.5,
            Heading = 90.0,
            DeliveryId = 100,
        };

        var exception = new Exception("Database error");
        _mockDriverRepository
            .Setup(x => x.AddDriverLocationHistoryAsync(It.IsAny<DriverLocationHistory>()))
            .ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command));
        thrownException.Should().Be(exception);

        // Verify error logging
        _mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (v, t) =>
                            v.ToString()!
                                .Contains(
                                    $"Error adding location history for driver {command.DriverId}"
                                )
                    ),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldLogDebugMessage()
    {
        // Arrange
        var command = new AddDriverLocationHistoryCommand
        {
            DriverId = 1,
            Location = new Location { Latitude = 51.5074, Longitude = -0.1278 },
            Accuracy = 5.0,
            Speed = 10.5,
            Heading = 90.0,
            DeliveryId = 100,
        };

        _mockDriverRepository
            .Setup(x => x.AddDriverLocationHistoryAsync(It.IsAny<DriverLocationHistory>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command);

        // Assert
        _mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (v, t) =>
                            v.ToString()!
                                .Contains($"Added location history for driver {command.DriverId}")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }
}

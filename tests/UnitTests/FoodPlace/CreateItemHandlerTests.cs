using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FoodDeliveryAppAPI.Tests.UnitTests.FoodPlaceTests;

public class CreateItemHandlerTests
{
    private readonly Mock<IFoodPlaceRepository> _foodPlaceRepositoryMock;
    private readonly Mock<ILogger<CreateItemHandler>> _loggerMock;
    private readonly CreateItemHandler _handler;

    public CreateItemHandlerTests()
    {
        _foodPlaceRepositoryMock = new Mock<IFoodPlaceRepository>();
        _loggerMock = new Mock<ILogger<CreateItemHandler>>();
        _handler = new CreateItemHandler(_foodPlaceRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateItemSuccessfully()
    {
        // Arrange
        var req = new CreateItemCommand
        {
            Name = "Test Item",
            Description = "Test Description",
            Price = 360,
            IsAvailable = true,
        };

        var foodPlace = new FoodPlace
        {
            Id = 1,
            Name = "Test Food Place",
            Description = "Test Description",
            Category = "Test Category",
            AddressId = 1,
            Location = new Location { Latitude = 0, Longitude = 0 },
            Items = new List<FoodPlaceItem>(),
        };
        var userId = 1;

        _foodPlaceRepositoryMock
            .Setup(repo => repo.GetFoodPlaceByUserId(userId))
            .ReturnsAsync(foodPlace);

        // Act
        await _handler.Handle(req, userId);

        // Assert
        _foodPlaceRepositoryMock.Verify(
            repo => repo.AddFoodPlaceItem(foodPlace.Id, It.IsAny<FoodPlaceItem>()),
            Times.Once
        );
        foodPlace.Items.Should().ContainSingle(item => item.Name == req.Name);
    }
}

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FoodDeliveryAppAPI.Tests.UnitTests.FoodPlaceTests;

public class UpdateItemHandlerTests
{
    private readonly Mock<IFoodPlaceRepository> _foodPlaceRepositoryMock;
    private readonly Mock<ILogger<UpdateItemHandler>> _loggerMock;
    private readonly UpdateItemHandler _handler;

    public UpdateItemHandlerTests()
    {
        _foodPlaceRepositoryMock = new Mock<IFoodPlaceRepository>();
        _loggerMock = new Mock<ILogger<UpdateItemHandler>>();
        _handler = new UpdateItemHandler(_foodPlaceRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateItemSuccessfully()
    {
        // Arrange
        var req = new UpdateItemCommand
        {
            Id = 1,
            Name = "Updated Test Name",
            Description = "Updated Description",
            Price = 360,
            IsAvailable = true,
        };
        var oldName = "Old Item Name";
        var oldDescription = "Old Description";
        var existingItem = new FoodPlaceItem
        {
            Id = req.Id,
            Name = oldName,
            Description = oldDescription,
            Price = 300,
            IsAvailable = false,
        };

        var foodPlace = new FoodPlace
        {
            Id = 1,
            Name = "Test Food Place",
            Description = "Test Description",
            Category = "Test Category",
            AddressId = 1,
            Location = new Location { Latitude = 0, Longitude = 0 },
            Items = new List<FoodPlaceItem>() { existingItem },
        };
        var userId = 1;

        _foodPlaceRepositoryMock
            .Setup(repo => repo.GetFoodPlaceByUserId(userId))
            .ReturnsAsync(foodPlace);

        // Act
        await _handler.Handle(req, userId);

        // Assert
        _foodPlaceRepositoryMock.Verify(
            repo =>
                repo.UpdateFoodPlaceItem(
                    It.Is<FoodPlaceItem>(item => item.Id == req.Id && item.Name == req.Name)
                ),
            Times.Once
        );
        foodPlace
            .Items.Should()
            .ContainSingle(item =>
                item.Id == existingItem.Id
                && item.Name == req.Name
                && item.Description == req.Description
                && item.Price == req.Price
                && item.IsAvailable == req.IsAvailable
            );
        foodPlace
            .Items.Should()
            .NotContain(item =>
                item.Name == oldName
                && item.Description == oldDescription
                && item.Price == existingItem.Price
                && item.IsAvailable == existingItem.IsAvailable
            );
    }
}

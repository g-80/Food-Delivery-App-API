using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FoodDeliveryAppAPI.Tests.UnitTests.OrderTests;

public class ProcessOrderHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IDemoOrderProcessingService> _demoOrderProcessingServiceMock;
    private readonly Mock<IPaymentService> _paymentServiceMock;
    private readonly Mock<ILogger<ProcessOrderHandler>> _loggerMock;
    private readonly ProcessOrderHandler _handler;

    public ProcessOrderHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _demoOrderProcessingServiceMock = new Mock<IDemoOrderProcessingService>();
        _paymentServiceMock = new Mock<IPaymentService>();
        _loggerMock = new Mock<ILogger<ProcessOrderHandler>>();

        _handler = new ProcessOrderHandler(
            _orderRepositoryMock.Object,
            _demoOrderProcessingServiceMock.Object,
            _paymentServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldProcessOrderInDemoMode_WhenCalled()
    {
        // Arrange
        var orderId = 123;
        var order = OrderTestsHelper.CreateTestOrder(orderId);

        _orderRepositoryMock.Setup(x => x.GetOrderById(orderId)).ReturnsAsync(order);
        _demoOrderProcessingServiceMock
            .Setup(x => x.ProcessDemoOrder(order))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(orderId);

        // Assert
        order.Status.Should().Be(OrderStatuses.pendingConfirmation);
        order.Payment!.Status.Should().Be(PaymentStatuses.Completed);

        _orderRepositoryMock.Verify(x => x.GetOrderById(orderId), Times.Once);
        _orderRepositoryMock.Verify(x => x.UpdateOrderStatus(order), Times.Once);
        _orderRepositoryMock.Verify(
            x => x.UpdatePaymentStatus(orderId, order.Payment),
            Times.Exactly(2)
        );

        _paymentServiceMock.Verify(
            x => x.CapturePaymentIntent(order.Payment.StripePaymentIntentId!),
            Times.Once
        );
        _demoOrderProcessingServiceMock.Verify(x => x.ProcessDemoOrder(order), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCapturePaymentBeforeStartingDemoProcessing()
    {
        // Arrange
        var orderId = 123;
        var order = OrderTestsHelper.CreateTestOrder(orderId);
        var callOrder = new List<string>();

        _orderRepositoryMock.Setup(x => x.GetOrderById(orderId)).ReturnsAsync(order);
        _paymentServiceMock
            .Setup(x => x.CapturePaymentIntent(It.IsAny<string>()))
            .Callback(() => callOrder.Add("CapturePayment"));
        _demoOrderProcessingServiceMock
            .Setup(x => x.ProcessDemoOrder(order))
            .Callback(() => callOrder.Add("ProcessDemoOrder"))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(orderId);

        // Assert
        callOrder.Should().ContainInOrder("CapturePayment", "ProcessDemoOrder");
    }

    [Fact]
    public async Task Handle_ShouldUpdatePaymentStatusToCompleted()
    {
        // Arrange
        var orderId = 123;
        var order = OrderTestsHelper.CreateTestOrder(orderId);

        _orderRepositoryMock.Setup(x => x.GetOrderById(orderId)).ReturnsAsync(order);

        // Act
        await _handler.Handle(orderId);

        // Assert
        order.Payment!.Status.Should().Be(PaymentStatuses.Completed);
    }
}

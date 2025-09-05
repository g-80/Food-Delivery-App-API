using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FoodDeliveryAppAPI.Tests.UnitTests.OrderTests;

public class ProcessOrderHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IOrderConfirmationService> _orderConfirmationServiceMock;
    private readonly Mock<IDeliveryAssignmentService> _deliveryAssignmentServiceMock;
    private readonly Mock<IOrderCancellationService> _orderCancellationServiceMock;
    private readonly Mock<IPaymentService> _paymentServiceMock;
    private readonly Mock<ILogger<ProcessOrderHandler>> _loggerMock;
    private readonly ProcessOrderHandler _handler;

    public ProcessOrderHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _orderConfirmationServiceMock = new Mock<IOrderConfirmationService>();
        _deliveryAssignmentServiceMock = new Mock<IDeliveryAssignmentService>();
        _orderCancellationServiceMock = new Mock<IOrderCancellationService>();
        _paymentServiceMock = new Mock<IPaymentService>();
        _loggerMock = new Mock<ILogger<ProcessOrderHandler>>();

        _handler = new ProcessOrderHandler(
            _orderRepositoryMock.Object,
            _orderConfirmationServiceMock.Object,
            _deliveryAssignmentServiceMock.Object,
            _orderCancellationServiceMock.Object,
            _paymentServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldProcessOrderSuccessfully_WhenConfirmationAndDeliveryAssignmentSucceed()
    {
        // Arrange
        var orderId = 123;
        var order = OrderTestsHelper.CreateTestOrder(orderId);

        _orderRepositoryMock.Setup(x => x.GetOrderById(orderId)).ReturnsAsync(order);
        _orderConfirmationServiceMock
            .Setup(x => x.RequestOrderConfirmation(order))
            .ReturnsAsync(true);
        _deliveryAssignmentServiceMock
            .Setup(x => x.InitiateDeliveryAssignment(order))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(orderId);

        // Assert
        order.Status.Should().Be(OrderStatuses.preparing);
        order.Payment!.Status.Should().Be(PaymentStatuses.Completed);
        order.Delivery.Should().NotBeNull();

        _orderRepositoryMock.Verify(x => x.GetOrderById(orderId), Times.Once);
        _orderRepositoryMock.Verify(x => x.UpdateOrderStatus(order), Times.Exactly(2));
        _orderRepositoryMock.Verify(
            x => x.UpdatePaymentStatus(orderId, order.Payment),
            Times.Exactly(2)
        );
        _orderRepositoryMock.Verify(x => x.AddDelivery(orderId, order.Delivery!), Times.Once);

        _orderConfirmationServiceMock.Verify(x => x.RequestOrderConfirmation(order), Times.Once);
        _deliveryAssignmentServiceMock.Verify(x => x.InitiateDeliveryAssignment(order), Times.Once);
        _paymentServiceMock.Verify(
            x => x.CapturePaymentIntent(order.Payment.StripePaymentIntentId!),
            Times.Once
        );
        _paymentServiceMock.Verify(x => x.CancelPaymentIntent(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCancelOrder_WhenOrderConfirmationFails()
    {
        // Arrange
        var orderId = 123;
        var order = OrderTestsHelper.CreateTestOrder(orderId);

        _orderRepositoryMock.Setup(x => x.GetOrderById(orderId)).ReturnsAsync(order);
        _orderConfirmationServiceMock
            .Setup(x => x.RequestOrderConfirmation(order))
            .ReturnsAsync(false);
        _orderCancellationServiceMock
            .Setup(x => x.CancelOrder(order, "Confirmation failed"))
            .Callback<Order, string>(
                (o, reason) =>
                {
                    o.Status = OrderStatuses.cancelled;
                    o.Payment!.Status = PaymentStatuses.Cancelled;
                }
            )
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(orderId);

        // Assert
        order.Status.Should().Be(OrderStatuses.cancelled);
        order.Payment!.Status.Should().Be(PaymentStatuses.Cancelled);

        _orderRepositoryMock.Verify(x => x.GetOrderById(orderId), Times.Once);
        _orderRepositoryMock.Verify(x => x.UpdateOrderStatus(order), Times.Once); // Only to pendingConfirmation, cancellation service is mocked
        _orderRepositoryMock.Verify(x => x.UpdatePaymentStatus(orderId, order.Payment), Times.Once);
        _orderRepositoryMock.Verify(
            x => x.AddDelivery(It.IsAny<int>(), It.IsAny<Delivery>()),
            Times.Never
        );

        _orderConfirmationServiceMock.Verify(x => x.RequestOrderConfirmation(order), Times.Once);
        _orderCancellationServiceMock.Verify(
            x => x.CancelOrder(order, "Confirmation failed"),
            Times.Once
        );
        _deliveryAssignmentServiceMock.Verify(
            x => x.InitiateDeliveryAssignment(order),
            Times.Never
        );
        _paymentServiceMock.Verify(x => x.CapturePaymentIntent(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCancelOrder_WhenDeliveryAssignmentFails()
    {
        // Arrange
        var orderId = 123;
        var order = OrderTestsHelper.CreateTestOrder(orderId);

        _orderRepositoryMock.Setup(x => x.GetOrderById(orderId)).ReturnsAsync(order);
        _orderConfirmationServiceMock
            .Setup(x => x.RequestOrderConfirmation(order))
            .ReturnsAsync(true);
        _deliveryAssignmentServiceMock
            .Setup(x => x.InitiateDeliveryAssignment(order))
            .ReturnsAsync(false);
        _orderCancellationServiceMock
            .Setup(x => x.CancelOrder(order, "Delivery assignment failed"))
            .Callback<Order, string>(
                (o, reason) =>
                {
                    o.Status = OrderStatuses.cancelled;
                    o.Payment!.Status = PaymentStatuses.Cancelled;
                }
            )
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(orderId);

        // Assert
        order.Status.Should().Be(OrderStatuses.cancelled);
        order.Payment!.Status.Should().Be(PaymentStatuses.Cancelled);
        order.Delivery.Should().NotBeNull();

        _orderRepositoryMock.Verify(x => x.GetOrderById(orderId), Times.Once);
        _orderRepositoryMock.Verify(x => x.UpdateOrderStatus(order), Times.Exactly(2)); // pendingConfirmation then preparing
        _orderRepositoryMock.Verify(x => x.UpdatePaymentStatus(orderId, order.Payment), Times.Once);
        _orderRepositoryMock.Verify(x => x.AddDelivery(orderId, order.Delivery!), Times.Once);

        _orderConfirmationServiceMock.Verify(x => x.RequestOrderConfirmation(order), Times.Once);
        _deliveryAssignmentServiceMock.Verify(x => x.InitiateDeliveryAssignment(order), Times.Once);
        _orderCancellationServiceMock.Verify(
            x => x.CancelOrder(order, "Delivery assignment failed"),
            Times.Once
        );
        _paymentServiceMock.Verify(x => x.CapturePaymentIntent(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldCreateDelivery_WhenOrderIsConfirmed()
    {
        // Arrange
        var orderId = 123;
        var order = OrderTestsHelper.CreateTestOrder(orderId);

        _orderRepositoryMock.Setup(x => x.GetOrderById(orderId)).ReturnsAsync(order);
        _orderConfirmationServiceMock
            .Setup(x => x.RequestOrderConfirmation(order))
            .ReturnsAsync(true);
        _deliveryAssignmentServiceMock
            .Setup(x => x.InitiateDeliveryAssignment(order))
            .ReturnsAsync(false); // Fail delivery to verify delivery was created

        // Act
        await _handler.Handle(orderId);

        // Assert
        order
            .Delivery.Should()
            .NotBeNull("CreateDelivery should be called after order confirmation");
        _orderRepositoryMock.Verify(x => x.AddDelivery(orderId, order.Delivery!), Times.Once);
    }
}

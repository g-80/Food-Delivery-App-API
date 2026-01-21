using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FoodDeliveryAppAPI.Tests.UnitTests.OrderTests;

public class OrderCancellationServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IPaymentService> _paymentServiceMock;
    private readonly Mock<IDeliveryAssignmentService> _deliveryAssignmentServiceMock;
    private readonly Mock<ILogger<OrderCancellationService>> _loggerMock;
    private readonly OrderCancellationService _service;

    public OrderCancellationServiceTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _paymentServiceMock = new Mock<IPaymentService>();
        _deliveryAssignmentServiceMock = new Mock<IDeliveryAssignmentService>();
        _loggerMock = new Mock<ILogger<OrderCancellationService>>();

        _service = new OrderCancellationService(
            _orderRepositoryMock.Object,
            _paymentServiceMock.Object,
            _deliveryAssignmentServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task CancelOrder_ShouldCancelPaymentIntent_WhenPaymentIsPendingCapture()
    {
        // Arrange
        var order = OrderTestsHelper.CreateTestOrder();
        order.Payment!.Status = PaymentStatuses.PendingCapture;

        // Act
        var result = await _service.CancelOrder(order, "Test reason");

        // Assert
        result.Should().BeTrue();
        order.Status.Should().Be(OrderStatuses.cancelled);
        order.Payment.Status.Should().Be(PaymentStatuses.Cancelled);

        _paymentServiceMock.Verify(x => x.CancelPaymentIntent("pi_test123"), Times.Once);
        _paymentServiceMock.Verify(
            x => x.RefundPayment(It.IsAny<string>(), It.IsAny<int>()),
            Times.Never
        );
        _orderRepositoryMock.Verify(x => x.UpdateOrderStatus(order), Times.Once);
        _orderRepositoryMock.Verify(
            x => x.UpdatePaymentStatus(order.Id, order.Payment),
            Times.Once
        );
    }

    [Fact]
    public async Task CancelOrder_ShouldRefundPayment_WhenPaymentIsCompleted()
    {
        // Arrange
        var order = OrderTestsHelper.CreateTestOrder();
        order.Payment!.Status = PaymentStatuses.Completed;

        // Act
        var result = await _service.CancelOrder(order, "Test reason");

        // Assert
        result.Should().BeTrue();
        order.Status.Should().Be(OrderStatuses.cancelled);
        order.Payment.Status.Should().Be(PaymentStatuses.Refunded);

        _paymentServiceMock.Verify(x => x.RefundPayment("pi_test123", 1400), Times.Once);
        _paymentServiceMock.Verify(x => x.CancelPaymentIntent(It.IsAny<string>()), Times.Never);
        _orderRepositoryMock.Verify(x => x.UpdateOrderStatus(order), Times.Once);
        _orderRepositoryMock.Verify(
            x => x.UpdatePaymentStatus(order.Id, order.Payment),
            Times.Once
        );
    }
}

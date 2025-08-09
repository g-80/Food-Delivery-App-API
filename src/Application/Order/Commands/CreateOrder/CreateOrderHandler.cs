using System.Transactions;
using Hangfire;

public class CreateOrderHandler
{
    private readonly IAddressRepository _addressRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IOrderRepository _ordersRepository;
    private readonly IOrderConfirmationService _orderConfirmationService;
    private readonly IDeliveryAssignmentService _deliveryAssignmentService;
    private readonly ILogger<CreateOrderHandler> _logger;

    public CreateOrderHandler(
        IAddressRepository addressRepository,
        ICartRepository cartRepository,
        IOrderRepository ordersRepository,
        IOrderConfirmationService orderConfirmationService,
        IDeliveryAssignmentService deliveryAssignmentService,
        ILogger<CreateOrderHandler> logger
    )
    {
        _addressRepository = addressRepository;
        _cartRepository = cartRepository;
        _ordersRepository = ordersRepository;
        _orderConfirmationService = orderConfirmationService;
        _deliveryAssignmentService = deliveryAssignmentService;
        _logger = logger;
    }

    public async Task<int> Handle(CreateOrderCommand command, int customerId)
    {
        var address = new Address
        {
            NumberAndStreet = command.DeliveryAddress.NumberAndStreet,
            City = command.DeliveryAddress.City,
            Postcode = command.DeliveryAddress.Postcode,
        };
        var orderId = await CreateOrder(customerId, address);

        BackgroundJob.Enqueue(() => ProcessOrderAsync(orderId));
        return orderId;
    }

    private async Task<int> CreateOrder(int customerId, Address deliveryAddress)
    {
        var addressId = await _addressRepository.AddAddress(deliveryAddress, customerId);

        Cart cart =
            await _cartRepository.GetCartByCustomerId(customerId)
            ?? throw new CartNotFoundException();

        if (!cart.Items.Any())
            throw new InvalidOperationException("Cart is empty");

        var cartPricing = cart.Pricing;

        int orderId;
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            var order = new Order
            {
                CustomerId = customerId,
                FoodPlaceId = cart.FoodPlaceId,
                DeliveryAddressId = addressId,
                Status = OrderStatuses.pending,
                Items = cart
                    .Items.Select(item => new OrderItem
                    {
                        ItemId = item.ItemId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        Subtotal = item.Subtotal,
                    })
                    .ToList(),
                Subtotal = cartPricing.Subtotal,
                ServiceFee = cartPricing.ServiceFee,
                DeliveryFee = cartPricing.DeliveryFee,
                Total = cartPricing.Total,
                CreatedAt = DateTime.UtcNow,
            };
            orderId = await _ordersRepository.AddOrder(order);
            cart.ClearCart();
            await _cartRepository.UpdateCart(cart);
            scope.Complete();
        }
        _logger.LogInformation(
            "Order created with ID: {OrderId} for customer ID: {CustomerId}",
            orderId,
            customerId
        );
        return orderId;
    }

    public async Task ProcessOrderAsync(int orderId)
    {
        _logger.LogInformation("Processing order ID: {OrderId}", orderId);
        var order = await _ordersRepository.GetOrderById(orderId);
        var isConfirmed = await _orderConfirmationService.RequestOrderConfirmation(order!);

        if (!isConfirmed)
        {
            order!.Status = OrderStatuses.cancelled;
            // notify customer about cancellation
            // initiate refund
            await _ordersRepository.UpdateOrderStatus(order);
            _logger.LogInformation(
                "Order ID: {OrderId} was cancelled after confirmation failed",
                orderId
            );
            return;
        }
        _logger.LogInformation("Order ID: {OrderId} confirmed, proceeding to preparation", orderId);
        order!.Status = OrderStatuses.preparing;
        await _ordersRepository.UpdateOrderStatus(order);
        _logger.LogInformation(
            "Order ID: {OrderId} status updated to {OrderStatus}",
            orderId,
            order.Status
        );
        order.CreateDelivery();
        await _ordersRepository.AddDelivery(order.Id, order.Delivery!);

        await _deliveryAssignmentService.InitiateDeliveryAssignment(order);
    }
}

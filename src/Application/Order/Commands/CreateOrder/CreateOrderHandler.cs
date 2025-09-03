using System.Transactions;

public class CreateOrderHandler
{
    private readonly IAddressRepository _addressRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IOrderRepository _ordersRepository;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<CreateOrderHandler> _logger;

    public CreateOrderHandler(
        IAddressRepository addressRepository,
        ICartRepository cartRepository,
        IOrderRepository ordersRepository,
        IPaymentService paymentService,
        ILogger<CreateOrderHandler> logger
    )
    {
        _addressRepository = addressRepository;
        _cartRepository = cartRepository;
        _ordersRepository = ordersRepository;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<CreateOrderDTO> Handle(int customerId, CreateOrderCommand command)
    {
        var address = new Address
        {
            NumberAndStreet = command.DeliveryAddress.NumberAndStreet,
            City = command.DeliveryAddress.City,
            Postcode = command.DeliveryAddress.Postcode,
        };
        var order = await CreateOrder(customerId, address);
        var payment = CreatePayment(order);
        var intent = _paymentService.CreatePaymentIntent(order, address);
        payment.StripePaymentIntentId = intent.Id;
        order.Payment = payment;
        await _ordersRepository.AddPayment(order.Id, payment);
        return new CreateOrderDTO { OrderId = order.Id, ClientSecret = intent.ClientSecret };
    }

    private async Task<Order> CreateOrder(int customerId, Address deliveryAddress)
    {
        Cart cart = await _cartRepository.GetCartByCustomerId(customerId);

        if (!cart.Items.Any())
            throw new InvalidOperationException("Cart is empty");

        var cartPricing = cart.Pricing;

        Order? order = null;
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            var addressId = await _addressRepository.AddAddress(deliveryAddress, customerId);
            order = new Order
            {
                CustomerId = customerId,
                FoodPlaceId = cart.FoodPlaceId,
                DeliveryAddressId = addressId,
                Status = OrderStatuses.pendingPayment,
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
            order.Id = await _ordersRepository.AddOrder(order);
            cart.ClearCart();
            await _cartRepository.UpdateCart(cart);
            scope.Complete();
        }
        _logger.LogInformation(
            "Order created with ID: {OrderId} for customer ID: {CustomerId}",
            order.Id,
            customerId
        );
        return order;
    }

    private Payment CreatePayment(Order order)
    {
        var payment = new Payment { Amount = order.Total, Status = PaymentStatuses.NotConfirmed };
        _logger.LogInformation(
            "Saved payment record for orderId: {OrderId} for customerId: {CustomerId}",
            order.Id,
            order.CustomerId
        );
        return payment;
    }
}

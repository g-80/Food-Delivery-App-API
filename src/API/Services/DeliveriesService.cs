public class DeliveriesService
{
    private readonly DeliveriesRepository _deliveriesRepo;
    private readonly IOrdersRepository _ordersRepo;

    public DeliveriesService(DeliveriesRepository deliveriesRepo, IOrdersRepository ordersRepo)
    {
        _deliveriesRepo = deliveriesRepo;
        _ordersRepo = ordersRepo;
    }

    public async Task CreateDeliveryAsync(int orderId)
    {
        await _deliveriesRepo.CreateDelivery(
            new CreateDeliveryDTO
            {
                OrderId = orderId,
                AddressId = (await _ordersRepo.GetOrderById(orderId))!.DeliveryAddressId,
                ConfirmationCode = new Random().Next(1000, 9999).ToString(),
                Status = DeliveryStatuses.assigningDriver,
            }
        );
    }

    public async Task UpdateDeliveryDriverAsync(int orderId, int driverId)
    {
        await _deliveriesRepo.UpdateDeliveryDriver(orderId, driverId);
    }
}

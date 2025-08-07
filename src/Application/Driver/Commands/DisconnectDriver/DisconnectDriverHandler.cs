public class DisconnectDriverHandler
{
    private readonly IDriverRepository _driverRepository;

    public DisconnectDriverHandler(IDriverRepository driverRepository)
    {
        _driverRepository = driverRepository;
    }

    public async Task Handle(int driverId)
    {
        await _driverRepository.DisconnectDriver(driverId);
    }
}

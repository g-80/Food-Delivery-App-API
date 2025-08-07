public class GoOnlineHandler
{
    private readonly IDriverRepository _driverRepository;

    public GoOnlineHandler(IDriverRepository driverRepository)
    {
        _driverRepository = driverRepository;
    }

    public async Task Handle(int driverId)
    {
        await _driverRepository.ConnectDriver(driverId);
    }
}

public class UpsertLocationHandler
{
    private readonly IDriverRepository _driverRepository;

    public UpsertLocationHandler(IDriverRepository driverRepository)
    {
        _driverRepository = driverRepository;
    }

    public async Task Handle(int driverId, Location location)
    {
        var driver = await _driverRepository.GetDriverById(driverId);
        if (driver == null)
        {
            throw new InvalidOperationException($"Driver with ID: {driverId} not found.");
        }

        driver.Location = location;

        await _driverRepository.UpsertDriverLocation(driver);
    }
}

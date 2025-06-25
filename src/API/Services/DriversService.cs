public class DriversService
{
    private readonly DriversRepository _driversRepo;
    private readonly DriversStatusesRepository _driversStatusesRepo;
    private readonly DriversLocationsRepository _driversLocationsRepo;

    public DriversService(
        DriversRepository driversRepository,
        DriversStatusesRepository driversStatusesRepository,
        DriversLocationsRepository driversLocationsRepository
    )
    {
        _driversRepo = driversRepository;
        _driversStatusesRepo = driversStatusesRepository;
        _driversLocationsRepo = driversLocationsRepository;
    }

    public async Task CreateDriverStatusAsync(int driverId)
    {
        await _driversStatusesRepo.CreateDriverStatus(driverId, DriverStatuses.online);
    }

    public async Task UpdateDriverStatus(int driverId, DriverStatuses newStatus)
    {
        await _driversStatusesRepo.UpdateDriverStatus(driverId, newStatus);
    }

    public async Task SetDriverOnlineAsync(int driverId)
    {
        await _driversStatusesRepo.UpdateDriverStatus(driverId, DriverStatuses.online);
    }

    public async Task RemoveDriverStatusAsync(int driverId)
    {
        await _driversStatusesRepo.RemoveDriverStatus(driverId);
    }

    public async Task UpsertDriverLocationAsync(int driverId, double latitude, double longitude)
    {
        await _driversLocationsRepo.UpsertDriverLocation(driverId, latitude, longitude);
    }

    public async Task RemoveDriverLocationAsync(int driverId)
    {
        await _driversLocationsRepo.RemoveDriverLocation(driverId);
    }

    public async Task<IEnumerable<AvailableDriverDTO>> GetAvailableDriversWithinDistance(
        double latitude,
        double longitude,
        int distance
    )
    {
        var availableDrivers = await _driversRepo.GetAvailableDriversWithinDistance(
            latitude,
            longitude,
            distance,
            DriverStatuses.online
        );
        return availableDrivers;
    }
}

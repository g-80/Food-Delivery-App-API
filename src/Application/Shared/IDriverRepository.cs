public interface IDriverRepository
{
    public Task<bool> ConnectDriver(int driverId, DriverStatuses status = DriverStatuses.online);
    public Task DisconnectDriver(int driverId);
    public Task<IEnumerable<AvailableDriver>> GetAvailableDriversWithinDistance(
        double latitude,
        double longitude,
        int distance,
        DriverStatuses status = DriverStatuses.online
    );
    public Task<Driver?> GetDriverById(int id);
    public Task UpdateDriverStatus(Driver driver);
    public Task UpsertDriverLocation(Driver driver);
}

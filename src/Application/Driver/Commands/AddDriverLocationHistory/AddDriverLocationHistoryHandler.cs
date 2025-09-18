public class AddDriverLocationHistoryHandler
{
    private readonly IDriverRepository _driverRepository;
    private readonly ILogger<AddDriverLocationHistoryHandler> _logger;

    public AddDriverLocationHistoryHandler(
        IDriverRepository driverRepository,
        ILogger<AddDriverLocationHistoryHandler> logger
    )
    {
        _driverRepository = driverRepository;
        _logger = logger;
    }

    public async Task Handle(AddDriverLocationHistoryCommand command)
    {
        try
        {
            var locationHistory = new DriverLocationHistory
            {
                Id = 0,
                DriverId = command.DriverId,
                Location = command.Location,
                Accuracy = command.Accuracy,
                Speed = command.Speed,
                Heading = command.Heading,
                Timestamp = DateTime.UtcNow,
                DeliveryId = command.DeliveryId,
            };

            await _driverRepository.AddDriverLocationHistoryAsync(locationHistory);

            _logger.LogDebug(
                "Added location history for driver {DriverId} with accuracy {Accuracy}m",
                command.DriverId,
                command.Accuracy
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error adding location history for driver {DriverId}",
                command.DriverId
            );
            throw;
        }
    }
}

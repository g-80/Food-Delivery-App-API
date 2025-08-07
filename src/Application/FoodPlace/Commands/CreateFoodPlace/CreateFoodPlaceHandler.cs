public class CreateFoodPlaceHandler
{
    private readonly IFoodPlaceRepository _foodPlaceRepository;
    private readonly IAddressRepository _addressRepository;

    public CreateFoodPlaceHandler(
        IFoodPlaceRepository foodPlaceRepository,
        IAddressRepository addressRepository
    )
    {
        _foodPlaceRepository = foodPlaceRepository;
        _addressRepository = addressRepository;
    }

    public async Task<int> Handle(CreateFoodPlaceCommand req, int userId)
    {
        var addressId = await _addressRepository.AddAddress(
            new Address
            {
                NumberAndStreet = req.Address.NumberAndStreet,
                City = req.Address.City,
                Postcode = req.Address.Postcode,
            },
            userId
        );
        var foodPlace = new FoodPlace
        {
            Name = req.Name,
            Description = req.Description,
            Category = req.Category,
            AddressId = addressId,
            Location = new Location { Latitude = req.Latitude, Longitude = req.Longitude },
        };

        return await _foodPlaceRepository.AddFoodPlace(foodPlace, userId);
    }
}

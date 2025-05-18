public class AddressesService
{
    private readonly AddressesRepository _addressRepo;

    public AddressesService(AddressesRepository addressRepo)
    {
        _addressRepo = addressRepo;
    }

    public async Task<int> CreateAddress(AddressCreateRequest request, int userId)
    {
        var dto = new CreateAddressDTO
        {
            UserId = userId,
            NumberAndStreet = request.NumberAndSteet,
            City = request.City,
            Postcode = request.Postcode,
            IsPrimary = true,
        };

        return await _addressRepo.CreateAddress(dto);
    }

    public async Task<Address?> GetAddressById(int id)
    {
        return await _addressRepo.GetAddressById(id);
    }
}

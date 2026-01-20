using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

public class AddressRepository : BaseRepository, IAddressRepository
{
    public AddressRepository(IOptions<DatabaseOptions> options)
        : base(options.Value.ConnectionString) { }

    public async Task<Address?> GetAddressById(int id)
    {
        var parameters = new { Id = id };

        const string sql =
            @"
            SELECT *
            FROM addresses
            WHERE id = @Id
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            return await connection.QuerySingleOrDefaultAsync<Address>(sql, parameters);
        }
        ;
    }

    public async Task<int> AddAddress(Address address, int userId)
    {
        var dto = new
        {
            UserId = userId,
            address.NumberAndStreet,
            address.City,
            address.Postcode,
        };
        var parameters = dto;

        // Use a CTE to insert the address and return the ID if it was inserted,
        // or select the existing ID if it already exists.
        // ON CONFLICT DO NOTHING will cause no row returns.
        // So we need another SELECT to get the existing id.
        // The second SELECT will get the existing row.
        // if it inserts successfully, then there will be two same records, then we need UNION to merge the result.

        const string sql =
            @"
            WITH inserted_address AS (
            INSERT INTO addresses (user_id, number_and_street, city, postcode)
            VALUES (@UserId, @NumberAndStreet, @City, @Postcode)
            ON CONFLICT DO NOTHING
            RETURNING id
            )
            SELECT *
            FROM inserted_address
            UNION
            SELECT id
            FROM addresses
            WHERE user_id = @UserId AND number_and_street = @NumberAndStreet AND city = @City AND postcode = @Postcode
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            return await connection.QuerySingleAsync<int>(sql, parameters);
        }
    }
}

using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

public class AddressesRepository : BaseRepository
{
    public AddressesRepository(IOptions<DatabaseOptions> options)
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
            return await connection.QuerySingleOrDefaultAsync<Address>(sql, parameters);
        }
        ;
    }

    public async Task<int> CreateAddress(CreateAddressDTO dto)
    {
        var parameters = dto;

        const string sql =
            @"
            INSERT INTO addresses (user_id, number_and_street, city, postcode, is_primary)
            VALUES (@UserId, @NumberAndStreet, @City, @Postcode, @IsPrimary)
            RETURNING id
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            return await connection.ExecuteAsync(sql, parameters);
        }
    }
}

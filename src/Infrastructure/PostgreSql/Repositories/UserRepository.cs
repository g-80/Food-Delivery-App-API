using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

public class UserRepository : BaseRepository, IUserRepository
{
    public UserRepository(IOptions<DatabaseOptions> options)
        : base(options.Value.ConnectionString) { }

    public async Task<int> AddUser(User user)
    {
        var parameters = new
        {
            user.FirstName,
            user.Surname,
            user.PhoneNumber,
            user.Password,
            user.UserType,
        };
        const string sql =
            @"
            INSERT INTO users(first_name, surname, phone_number, password, user_type)
            VALUES
            (@FirstName, @Surname, @PhoneNumber, @Password, @UserType)
            RETURNING id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            return await connection.ExecuteScalarAsync<int>(sql, parameters);
        }
        ;
    }

    public async Task<User?> GetUserById(int id)
    {
        var parameters = new { Id = id };
        const string sql =
            @"
            SELECT *
            FROM users
            WHERE id = @Id
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            return await connection.QuerySingleOrDefaultAsync<User>(sql, parameters);
        }
        ;
    }

    public async Task<User?> GetUserByPhoneNumber(string phoneNumber)
    {
        var parameters = new { number = phoneNumber };
        const string sql =
            @"
            SELECT *
            FROM users
            WHERE phone_number = @number
            ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            return await connection.QuerySingleOrDefaultAsync<User>(sql, parameters);
        }
        ;
    }

    public async Task<bool> UpdateUser(User user)
    {
        var parameters = new
        {
            user.FirstName,
            user.Surname,
            user.PhoneNumber,
            user.Password,
            user.UserType,
        };

        const string sql =
            @"
            UPDATE users
            SET first_name = @FirstName, surname = @Surname, phone_number = @PhoneNumber, password = @Password
            WHERE id = @Id
        ";
        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            int rowsAffected = await connection.ExecuteAsync(sql, parameters);
            return rowsAffected > 0;
        }
        ;
    }
}

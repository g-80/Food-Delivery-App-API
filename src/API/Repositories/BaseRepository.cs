public abstract class BaseRepository
{
    protected readonly string _connectionString;

    public BaseRepository(string connectionString)
    {
        _connectionString = connectionString;
    }
}

public class BaseRepo
{
    protected readonly string _connectionString;
    public BaseRepo(string connectionString)
    {
        _connectionString = connectionString;
    }
}
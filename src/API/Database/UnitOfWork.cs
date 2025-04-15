using Npgsql;

public class UnitOfWork : IDisposable
{
    private readonly string _connectionString;

    public UnitOfWork(string connectionString)
    {
        _connectionString = connectionString;
    }

    public NpgsqlConnection? Connection { get; private set; }
    public NpgsqlTransaction? Transaction { get; private set; }

    public void BeginTransaction()
    {
        if (Connection == null)
        {
            Connection = new NpgsqlConnection(_connectionString);
            Connection.Open();
            Transaction = Connection.BeginTransaction();
        }
    }

    public void Commit()
    {
        Transaction?.Commit();
        DisposeTransaction();
    }

    public void Rollback()
    {
        Transaction?.Rollback();
        DisposeTransaction();
    }

    private void DisposeTransaction()
    {
        if (Transaction != null)
        {
            Transaction.Dispose();
            Transaction = null;
        }
    }

    public void Dispose()
    {
        Connection?.Dispose();
        Transaction?.Dispose();
        Transaction = null;
        Connection = null;
    }
}

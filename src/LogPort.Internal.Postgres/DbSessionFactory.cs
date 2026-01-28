using LogPort.Internal.Abstractions;
using LogPort.Internal.Configuration;

namespace LogPort.Data.Postgres;

public sealed class DbSessionFactory : IDbSessionFactory
{
    private readonly string _connectionString;
    public DbSessionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    public DbSessionFactory(LogPortConfig config) : this(config.Postgres.ConnectionString) {}
    public IDbSession Create() => new DbSession(_connectionString);
}

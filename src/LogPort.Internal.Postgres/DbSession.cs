using Dapper;
using Npgsql;

namespace LogPort.Data.Postgres;


public sealed class DbSession : IAsyncDisposable
{
    private readonly NpgsqlConnection _connection;
    private NpgsqlTransaction? _transaction;

    public DbSession(string connectionString)
    {
        _connection = new NpgsqlConnection(connectionString);
    }

    public async Task OpenAsync(CancellationToken ct = default)
    {
        if (_connection.State != System.Data.ConnectionState.Open)
            await _connection.OpenAsync(ct);
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _transaction = await _connection.BeginTransactionAsync(ct);
    }

    public async Task CommitAsync()
    {
        if (_transaction == null)
            return;

        await _transaction.CommitAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public Task<int> ExecuteAsync(SqlCommand command, CancellationToken ct = default) =>
        _connection.ExecuteAsync(
            new CommandDefinition(
                command.Sql,
                command.Parameters,
                _transaction,
                cancellationToken: ct));

    public Task<T> ExecuteScalarAsync<T>(SqlCommand command, CancellationToken ct = default) =>
        _connection.ExecuteScalarAsync<T>(
            new CommandDefinition(
                command.Sql,
                command.Parameters,
                _transaction,
                cancellationToken: ct));

    public Task<IEnumerable<T>> QueryAsync<T>(SqlCommand command, CancellationToken ct = default) =>
        _connection.QueryAsync<T>(
            new CommandDefinition(
                command.Sql,
                command.Parameters,
                _transaction,
                cancellationToken: ct));

    public Task<T> QuerySingleAsync<T>(SqlCommand command, CancellationToken ct = default) =>
        _connection.QuerySingleAsync<T>(
            new CommandDefinition(
                command.Sql,
                command.Parameters,
                _transaction,
                cancellationToken: ct));

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
            await _transaction.DisposeAsync();

        await _connection.DisposeAsync();
    }
}
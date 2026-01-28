using System.Data;
using Dapper;
using LogPort.Internal;
using LogPort.Internal.Abstractions;
using Npgsql;

namespace LogPort.Data.Postgres;

public sealed class DbSession : IAsyncDisposable, IDbSession
{
    private readonly NpgsqlConnection _connection;
    private NpgsqlTransaction? _transaction;
    private int _transactionDepth;

    public DbSession(string connectionString)
    {
        _connection = new NpgsqlConnection(connectionString);
        _transactionDepth = 0;
    }

    public async Task OpenAsync(CancellationToken ct = default)
    {
        if (_connection.State != ConnectionState.Open)
            await _connection.OpenAsync(ct);
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_transactionDepth == 0)
        {
            _transaction = await _connection.BeginTransactionAsync(ct);
        }
        else
        {
            var savepoint = $"sp_{_transactionDepth}";
            await _connection.ExecuteAsync($"SAVEPOINT {savepoint}", transaction: _transaction, commandTimeout: null);
        }

        _transactionDepth++;
    }

    public async Task CommitAsync()
    {
        if (_transactionDepth == 0)
            return;

        _transactionDepth--;

        if (_transactionDepth == 0)
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        // else inner commit; do nothing, savepoint released automatically in Postgres
    }

    public async Task RollbackAsync()
    {
        if (_transactionDepth == 0)
            return;

        if (_transactionDepth == 1)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        else
        {
            var savepoint = $"sp_{_transactionDepth - 1}";
            await _connection.ExecuteAsync($"ROLLBACK TO SAVEPOINT {savepoint}", transaction: _transaction, commandTimeout: null);
        }

        _transactionDepth--;
    }

    public Task<int> ExecuteAsync(SqlCommand command, CancellationToken ct = default) =>
        _connection.ExecuteAsync(
            new CommandDefinition(
                command.Sql,
                command.Parameters,
                _transaction,
                cancellationToken: ct));

    public Task<T?> ExecuteScalarAsync<T>(SqlCommand command, CancellationToken ct = default) =>
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

using Dapper;

using LogPort.Internal;
using LogPort.Internal.Abstractions;

using Npgsql;

namespace LogPort.Data.Postgres;

/// <summary>
/// Represents an asynchronous database session for PostgreSQL,
/// encapsulating a connection and an optional transaction.
/// </summary>
public sealed class DbSession : IAsyncDisposable, IDbSession
{
    private readonly NpgsqlConnection _connection;
    private NpgsqlTransaction? _transaction;

    /// <summary>
    /// Initializes a new <see cref="DbSession"/> using the provided connection string.
    /// </summary>
    /// <param name="connectionString">The PostgreSQL connection string.</param>
    public DbSession(string connectionString)
    {
        _connection = new NpgsqlConnection(connectionString);
    }

    /// <summary>
    /// Opens the underlying database connection if it is not already open.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    public async Task OpenAsync(CancellationToken ct = default)
    {
        if (_connection.State != System.Data.ConnectionState.Open)
            await _connection.OpenAsync(ct);
    }

    /// <summary>
    /// Begins a database transaction on the current connection.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _transaction = await _connection.BeginTransactionAsync(ct);
    }

    /// <summary>
    /// Commits the active transaction, if one exists, and disposes it.
    /// </summary>
    public async Task CommitAsync()
    {
        if (_transaction == null)
            return;

        await _transaction.CommitAsync();
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    /// <summary>
    /// Executes a command that does not return a result set.
    /// </summary>
    /// <param name="command">The SQL command to execute.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    public Task<int> ExecuteAsync(SqlCommand command, CancellationToken ct = default) =>
        _connection.ExecuteAsync(
            new CommandDefinition(
                command.Sql,
                command.Parameters,
                _transaction,
                cancellationToken: ct));

    /// <summary>
    /// Executes a command and returns a single scalar value.
    /// </summary>
    /// <typeparam name="T">The expected scalar result type.</typeparam>
    /// <param name="command">The SQL command to execute.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The scalar result.</returns>
    public Task<T?> ExecuteScalarAsync<T>(SqlCommand command, CancellationToken ct = default) =>
        _connection.ExecuteScalarAsync<T>(
            new CommandDefinition(
                command.Sql,
                command.Parameters,
                _transaction,
                cancellationToken: ct));

    /// <summary>
    /// Executes a query and returns a sequence of results.
    /// </summary>
    /// <typeparam name="T">The result element type.</typeparam>
    /// <param name="command">The SQL command to execute.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>An enumerable of results.</returns>
    public Task<IEnumerable<T>> QueryAsync<T>(SqlCommand command, CancellationToken ct = default) =>
        _connection.QueryAsync<T>(
            new CommandDefinition(
                command.Sql,
                command.Parameters,
                _transaction,
                cancellationToken: ct));

    /// <summary>
    /// Executes a query and returns exactly one result.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="command">The SQL command to execute.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The single result.</returns>
    public Task<T> QuerySingleAsync<T>(SqlCommand command, CancellationToken ct = default) =>
        _connection.QuerySingleAsync<T>(
            new CommandDefinition(
                command.Sql,
                command.Parameters,
                _transaction,
                cancellationToken: ct));

    /// <summary>
    /// Disposes the transaction (rolling back if uncommitted) and the underlying connection.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
            await _transaction.DisposeAsync();

        await _connection.DisposeAsync();
    }
}

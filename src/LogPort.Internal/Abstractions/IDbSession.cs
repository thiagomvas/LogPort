namespace LogPort.Internal.Abstractions;


public interface IDbSession : IAsyncDisposable
{
    /// <summary>
    /// Opens the underlying database connection if it is not already open.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    Task OpenAsync(CancellationToken ct = default);

    /// <summary>
    /// Begins a database transaction on the current connection.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    Task BeginTransactionAsync(CancellationToken ct = default);

    /// <summary>
    /// Commits the active transaction, if one exists, and disposes it.
    /// </summary>
    Task CommitAsync();

    /// <summary>
    /// Executes a command that does not return a result set.
    /// </summary>
    /// <param name="command">The SQL command to execute.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The number of rows affected.</returns>
    Task<int> ExecuteAsync(SqlCommand command, CancellationToken ct = default);

    /// <summary>
    /// Executes a command and returns a single scalar value.
    /// </summary>
    /// <typeparam name="T">The expected scalar result type.</typeparam>
    /// <param name="command">The SQL command to execute.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The scalar result.</returns>
    Task<T?> ExecuteScalarAsync<T>(SqlCommand command, CancellationToken ct = default);

    /// <summary>
    /// Executes a query and returns a sequence of results.
    /// </summary>
    /// <typeparam name="T">The result element type.</typeparam>
    /// <param name="command">The SQL command to execute.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>An enumerable of results.</returns>
    Task<IEnumerable<T>> QueryAsync<T>(SqlCommand command, CancellationToken ct = default);

    /// <summary>
    /// Executes a query and returns exactly one result.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="command">The SQL command to execute.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>The single result.</returns>
    Task<T> QuerySingleAsync<T>(SqlCommand command, CancellationToken ct = default);
}
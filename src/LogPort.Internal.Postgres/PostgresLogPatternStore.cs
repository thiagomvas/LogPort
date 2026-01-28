using LogPort.Core;
using LogPort.Core.Models;
using LogPort.Data.Postgres.Commands;
using LogPort.Internal.Abstractions;
using LogPort.Internal.Configuration;
using LogPort.SDK;

using Microsoft.Extensions.Logging;

namespace LogPort.Data.Postgres;

public sealed class PostgresLogPatternStore : ILogPatternStore
{
    private readonly string _connectionString;
    private readonly LogNormalizer _normalizer;
    private readonly ILogger<PostgresLogPatternStore>? _logger;
    private readonly IDbSessionFactory _sessionFactory;


    public PostgresLogPatternStore(string connectionString, LogNormalizer normalizer, IDbSessionFactory sessionFactory, ILogger<PostgresLogPatternStore>? logger = null)
    {
        _connectionString = connectionString;
        _normalizer = normalizer;
        _sessionFactory = sessionFactory;
        _logger = logger;
    }
    
    public PostgresLogPatternStore(LogPortConfig config, LogNormalizer normalizer, IDbSessionFactory sessionFactory, ILogger<PostgresLogPatternStore>? logger = null) 
        : this(config.Postgres.ConnectionString, normalizer, sessionFactory, logger)
    {}

    public async Task<long> UpsertAsync(
        string message,
        ulong patternHash,
        DateTime timestamp,
        string level = "INFO",
        CancellationToken cancellationToken = default)
    {
        var normalizedMessage = _normalizer.NormalizeMessage(message);

        await using var session = _sessionFactory.Create();
        await session.OpenAsync(cancellationToken);
        await session.BeginTransactionAsync(cancellationToken);

        try
        {
            var command = UpsertPatternCommand.Create(
                normalizedMessage,
                patternHash,
                timestamp,
                level);

            var result = await session.QuerySingleAsync<long>(command, cancellationToken);

            await session.CommitAsync();

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger?.LogWarning("Upsert cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(
                ex,
                "Failed to upsert pattern '{Message}' with hash {Hash}",
                normalizedMessage,
                patternHash);

            throw;
        }
    }
}

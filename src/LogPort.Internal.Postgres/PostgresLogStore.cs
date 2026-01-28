using System.Runtime.CompilerServices;
using System.Text.Json;

using LogPort.Core;
using LogPort.Core.Models;
using LogPort.Data.Postgres.Commands;
using LogPort.Internal.Abstractions;
using LogPort.Internal.Configuration;

using Microsoft.Extensions.Logging;

namespace LogPort.Data.Postgres;

public sealed class PostgresLogStore : ILogStore
{
    private readonly IDbSessionFactory _sessionFactory;
    private readonly ILogPatternStore _patternStore;
    private readonly LogNormalizer _normalizer;
    private readonly PartitionManager _partitionManager;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<PostgresLogStore>? _logger;

    public PostgresLogStore(
        LogPortConfig config,
        IDbSessionFactory sessionFactory,
        ILogPatternStore patternStore,
        LogNormalizer normalizer,
        JsonSerializerOptions? jsonOptions = null)
    {
        _sessionFactory = sessionFactory;
        _patternStore = patternStore;
        _normalizer = normalizer;
        _partitionManager = new PartitionManager(config.Postgres.PartitionLength);
        _jsonOptions = jsonOptions ?? new JsonSerializerOptions();
    }

    public Task AddAsync(LogEntry log, CancellationToken cancellationToken = default)
        => AddBatchAsync([log], cancellationToken);

    public async Task AddBatchAsync(IReadOnlyCollection<LogEntry> logs, CancellationToken cancellationToken = default)
    {
        if (logs.Count == 0)
            return;

        await using var session = _sessionFactory.Create();
        await session.OpenAsync(cancellationToken);
        await session.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var day in logs.Select(l => l.Timestamp.Date).Distinct())
            {
                var partitionCmd = _partitionManager.EnsurePartition(day);
                await session.ExecuteAsync(partitionCmd, cancellationToken);
            }

            var patternCache = new Dictionary<ulong, long>();

            var logsWithPatterns = new List<LogEntry>(logs.Count);
            foreach (var log in logs)
            {
                var normalized = _normalizer.NormalizeMessage(log.Message, log.Metadata);
                var patternHash = LogNormalizer.ComputePatternHash(normalized);

                if (!patternCache.TryGetValue(patternHash, out var patternId))
                {
                    patternId = await _patternStore.UpsertAsync(normalized, patternHash, log.Timestamp,
                        _normalizer.NormalizeLevel(log.Level), cancellationToken);
                    patternCache[patternHash] = patternId;
                }

                logsWithPatterns.Add(new LogEntry
                {
                    Timestamp = log.Timestamp,
                    ServiceName = log.ServiceName,
                    Level = log.Level,
                    Message = log.Message,
                    Metadata = log.Metadata,
                    TraceId = log.TraceId,
                    SpanId = log.SpanId,
                    Hostname = log.Hostname,
                    Environment = log.Environment
                });
            }

            var command = AddLogBatchCommand.Create(logsWithPatterns);
            await session.ExecuteAsync(command, cancellationToken);

            await session.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to insert logs.");
            throw;
        }
    }


    public async Task<IReadOnlyList<LogEntry>> GetAsync(LogQueryParameters query,
        CancellationToken cancellationToken = default)
    {
        await using var session = _sessionFactory.Create();
        await session.OpenAsync(cancellationToken);
        await session.BeginTransactionAsync(cancellationToken);

        var command = GetLogsCommand.Create(query, _jsonOptions);
        var result = await session.QueryAsync<LogEntry>(command, cancellationToken);
        return result.ToList();
    }

    public IAsyncEnumerable<IReadOnlyList<LogEntry>> GetBatchesAsync(LogQueryParameters query, int batchSize,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<long> CountAsync(LogQueryParameters query, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
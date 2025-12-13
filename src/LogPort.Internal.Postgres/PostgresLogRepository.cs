using System.Data;
using LogPort.Core.Models;
using Npgsql;
using NpgsqlTypes;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using LogPort.Core;
using LogPort.Internal;
using LogPort.Internal.Abstractions;

namespace LogPort.Data.Postgres;


public class PostgresLogRepository : ILogRepository
{
    private readonly string _connectionString;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly int _partitionLengthInDays;
    private readonly LogNormalizer _normalizer;

    public PostgresLogRepository(LogPortConfig config, LogNormalizer normalizer,
        JsonSerializerOptions? jsonOptions = null)
    {
        _connectionString = config.Postgres.ConnectionString;
        _partitionLengthInDays = config.Postgres.PartitionLength;
        _jsonOptions = jsonOptions ?? new JsonSerializerOptions
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };
        _normalizer = normalizer;
    }

    public Task AddLogAsync(LogEntry log) => AddLogsAsync(new[] { log });

    public async Task AddLogsAsync(IEnumerable<LogEntry> logs)
{
    var logList = logs.ToList();
    if (logList.Count == 0)
        return;

    foreach (var day in logList.Select(l => l.Timestamp.Date).Distinct())
        await EnsurePartitionAsync(day);

    await using var conn = new NpgsqlConnection(_connectionString);
    await conn.OpenAsync();
    await using var tx = await conn.BeginTransactionAsync();

    // Cache patternHash -> patternId (per batch)
    var patternCache = new Dictionary<ulong, long>();

    var values = new List<string>();
    var parameters = new List<NpgsqlParameter>();
    int i = 0;

    foreach (var log in logList)
    {
        var normalized = _normalizer.NormalizeMessage(log.Message, log.Metadata);
        var hash = LogNormalizer.ComputePatternHash(normalized);

        if (!patternCache.TryGetValue(hash, out var patternId))
        {
            await using var patternCmd = new NpgsqlCommand(@"
INSERT INTO log_patterns (normalized_message, pattern_hash, first_seen, last_seen, occurrence_count)
VALUES (@msg, @hash, NOW(), NOW(), 1)
ON CONFLICT (pattern_hash)
DO UPDATE SET
  last_seen = NOW(),
  occurrence_count = log_patterns.occurrence_count + 1
RETURNING id;", conn, tx);

            patternCmd.Parameters.AddWithValue("msg", normalized);
            patternCmd.Parameters.AddWithValue("hash", (long)hash);

            patternId = (long)await patternCmd.ExecuteScalarAsync();
            patternCache[hash] = patternId;
        }

        values.Add(
            $"(@ts{i}, @svc{i}, @lvl{i}, @msg{i}, @meta{i}, @trace{i}, @span{i}, @host{i}, @env{i}, @pid{i})");

        parameters.AddRange(new[]
        {
            new NpgsqlParameter($"ts{i}", log.Timestamp),
            new NpgsqlParameter($"svc{i}", (object?)log.ServiceName ?? DBNull.Value),
            new NpgsqlParameter($"lvl{i}", _normalizer.NormalizeLevel(log.Level)),
            new NpgsqlParameter($"msg{i}", log.Message),
            new NpgsqlParameter($"meta{i}", NpgsqlDbType.Jsonb)
                { Value = JsonSerializer.Serialize(log.Metadata, _jsonOptions) },
            new NpgsqlParameter($"trace{i}", (object?)log.TraceId ?? DBNull.Value),
            new NpgsqlParameter($"span{i}", (object?)log.SpanId ?? DBNull.Value),
            new NpgsqlParameter($"host{i}", (object?)log.Hostname ?? DBNull.Value),
            new NpgsqlParameter($"env{i}", (object?)log.Environment ?? DBNull.Value),
            new NpgsqlParameter($"pid{i}", patternId)
        });

        i++;
    }

    var sql = $@"
INSERT INTO logs
(timestamp, service_name, level, message, metadata, trace_id, span_id, hostname, environment, pattern_id)
VALUES {string.Join(", ", values)};";

    await using var cmd = new NpgsqlCommand(sql, conn, tx);
    cmd.Parameters.AddRange(parameters.ToArray());
    await cmd.ExecuteNonQueryAsync();

    await tx.CommitAsync();
}


    public async Task<IEnumerable<LogEntry>> GetLogsAsync(LogQueryParameters parameters)
    {
        var sql = new StringBuilder(
            "SELECT timestamp, service_name, level, message, metadata, trace_id, span_id, hostname, environment FROM logs WHERE 1=1");
        var sqlParams = new List<NpgsqlParameter>();

        BuildFilters(sql, sqlParams, parameters);

        sql.Append(" ORDER BY timestamp DESC");

        var offset = ((parameters.Page ?? 1) - 1) * (parameters.PageSize ?? 100);
        sql.Append(" LIMIT @limit OFFSET @offset");
        sqlParams.Add(new NpgsqlParameter("limit", parameters.PageSize ?? 100));
        sqlParams.Add(new NpgsqlParameter("offset", offset));

        var results = new List<LogEntry>();
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql.ToString(), conn);
        cmd.Parameters.AddRange(sqlParams.ToArray());

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            results.Add(MapReader(reader));

        return results;
    }

    public async IAsyncEnumerable<IReadOnlyList<LogEntry>> GetBatchesAsync(
        LogQueryParameters parameters,
        int batchSize)
    {
        var sql = new StringBuilder(
            "SELECT timestamp, service_name, level, message, metadata, trace_id, span_id, hostname, environment FROM logs WHERE 1=1");
        var sqlParams = new List<NpgsqlParameter>();
        BuildFilters(sql, sqlParams, parameters);

        sql.Append(" ORDER BY timestamp ASC");

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql.ToString(), conn);
        cmd.Parameters.AddRange(sqlParams.ToArray());

        await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess);

        var batch = new List<LogEntry>(batchSize);
        while (await reader.ReadAsync())
        {
            batch.Add(MapReader(reader));

            if (batch.Count >= batchSize)
            {
                yield return batch;
                batch = new List<LogEntry>(batchSize);
            }
        }

        if (batch.Count > 0)
            yield return batch;
    }


    public async Task<long> CountLogsAsync(LogQueryParameters parameters)
    {
        var sql = new StringBuilder("SELECT COUNT(*) FROM logs WHERE 1=1");
        var sqlParams = new List<NpgsqlParameter>();
        BuildFilters(sql, sqlParams, parameters);

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql.ToString(), conn);
        cmd.Parameters.AddRange(sqlParams.ToArray());

        return (long)await cmd.ExecuteScalarAsync();
    }

    public async Task<LogMetadata> GetLogMetadataAsync()
    {
        const string sql = @"
WITH
  lvl_counts AS (
    SELECT jsonb_object_agg(level, count) AS data
    FROM (SELECT level, COUNT(*) AS count FROM logs WHERE level IS NOT NULL GROUP BY level) t
  ),
  svc_counts AS (
    SELECT jsonb_object_agg(service_name, count) AS data
    FROM (SELECT service_name, COUNT(*) AS count FROM logs WHERE service_name IS NOT NULL GROUP BY service_name) t
  ),
  env_counts AS (
    SELECT jsonb_object_agg(environment, count) AS data
    FROM (SELECT environment, COUNT(*) AS count FROM logs WHERE environment IS NOT NULL GROUP BY environment) t
  ),
  host_counts AS (
    SELECT jsonb_object_agg(hostname, count) AS data
    FROM (SELECT hostname, COUNT(*) AS count FROM logs WHERE hostname IS NOT NULL GROUP BY hostname) t
  ),
  distincts AS (
    SELECT
      array_agg(DISTINCT level) AS levels,
      array_agg(DISTINCT environment) AS environments,
      array_agg(DISTINCT service_name) AS services,
      array_agg(DISTINCT hostname) AS hostnames,
      COUNT(*) AS log_count
    FROM logs
  )
SELECT
  d.levels,
  d.environments,
  d.services,
  d.hostnames,
  d.log_count,
  l.data AS log_count_by_level,
  s.data AS log_count_by_service,
  e.data AS log_count_by_environment,
  h.data AS log_count_by_hostname
FROM distincts d, lvl_counts l, svc_counts s, env_counts e, host_counts h;

";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            throw new InvalidOperationException("Failed to read log metadata.");

        return new LogMetadata
        {
            LogLevels = reader.IsDBNull(0) ? [] : reader.GetFieldValue<string[]>(0),
            Environments = reader.IsDBNull(1) ? [] : reader.GetFieldValue<string[]>(1),
            Services = reader.IsDBNull(2) ? [] : reader.GetFieldValue<string[]>(2),
            Hostnames = reader.IsDBNull(3) ? [] : reader.GetFieldValue<string[]>(3),
            LogCount = reader.GetInt64(4),
            LogCountByLevel = reader.IsDBNull(5)
                ? new()
                : JsonSerializer.Deserialize<Dictionary<string, int>>(reader.GetString(5))!,
            LogCountByService = reader.IsDBNull(6)
                ? new()
                : JsonSerializer.Deserialize<Dictionary<string, int>>(reader.GetString(6))!,
            LogCountByEnvironment = reader.IsDBNull(7)
                ? new()
                : JsonSerializer.Deserialize<Dictionary<string, int>>(reader.GetString(7))!,
            LogCountByHostname = reader.IsDBNull(8)
                ? new()
                : JsonSerializer.Deserialize<Dictionary<string, int>>(reader.GetString(8))!
        };
    }


    public async Task<LogPattern?> GetPatternByHashAsync(string patternHash)
    {
        const string sql = @"
SELECT id, normalized_message, pattern_hash, first_seen, last_seen, occurrence_count
FROM log_patterns
WHERE pattern_hash = @hash;
";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("hash", patternHash);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        return MapPattern(reader);
    }
    
    public async Task<long> CreatePatternAsync(string normalizedMessage, string patternHash)
    {
        const string sql = @"
INSERT INTO log_patterns (normalized_message, pattern_hash, occurrence_count)
VALUES (@msg, @hash, 1)
RETURNING id;
";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("msg", normalizedMessage);
        cmd.Parameters.AddWithValue("hash", patternHash);

        return (long)await cmd.ExecuteScalarAsync();
    }
    
    public async Task<long> GetOrCreatePatternAsync(
        string normalizedMessage,
        string patternHash,
        DateTime timestamp)
    {
        const string sql = @"
INSERT INTO log_patterns (normalized_message, pattern_hash, first_seen, last_seen, occurrence_count)
VALUES (@msg, @hash, @ts, @ts, 1)
ON CONFLICT (pattern_hash)
DO UPDATE SET
    last_seen = EXCLUDED.last_seen,
    occurrence_count = log_patterns.occurrence_count + 1
RETURNING id;
";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("msg", normalizedMessage);
        cmd.Parameters.AddWithValue("hash", patternHash);
        cmd.Parameters.AddWithValue("ts", timestamp);

        return (long)await cmd.ExecuteScalarAsync();
    }
    
    public async Task UpdatePatternMessageAsync(long patternId, string normalizedMessage)
    {
        const string sql = @"
UPDATE log_patterns
SET normalized_message = @msg
WHERE id = @id;
";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", patternId);
        cmd.Parameters.AddWithValue("msg", normalizedMessage);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyList<LogPattern>> GetPatternsAsync(
        int limit = 100,
        int offset = 0)
    {
        const string sql = @"
SELECT id, normalized_message, pattern_hash, first_seen, last_seen, occurrence_count
FROM log_patterns
ORDER BY last_seen DESC
LIMIT @limit OFFSET @offset;
";

        var results = new List<LogPattern>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("limit", limit);
        cmd.Parameters.AddWithValue("offset", offset);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            results.Add(MapPattern(reader));

        return results;
    }

    public async Task DeletePatternAsync(long patternId)
    {
        const string sql = @"
DELETE FROM log_patterns
WHERE id = @id;
";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", patternId);

        await cmd.ExecuteNonQueryAsync();
    }


    private static LogPattern MapPattern(NpgsqlDataReader reader) =>
        new()
        {
            Id = reader.GetInt64(0),
            NormalizedMessage = reader.GetString(1),
            PatternHash = unchecked((ulong)reader.GetInt64(2)),

            FirstSeen = reader.GetDateTime(3),
            LastSeen = reader.GetDateTime(4),
            OccurrenceCount = reader.GetInt64(5)
        };

    private async Task EnsurePartitionAsync(DateTime timestamp)
    {
        var startDate =
            timestamp.Date.AddDays(-((timestamp.Date - DateTime.MinValue.Date).Days % _partitionLengthInDays));
        var endDate = startDate.AddDays(_partitionLengthInDays);

        var partitionName = $"logs_{startDate:yyyy_MM_dd}_{_partitionLengthInDays}d";

        var sql = $@"
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM pg_class WHERE relname = '{partitionName}'
                ) THEN
                    EXECUTE format(
                        'CREATE TABLE IF NOT EXISTS %I PARTITION OF logs
                         FOR VALUES FROM (%L) TO (%L);',
                        '{partitionName}', '{startDate:yyyy-MM-dd}', '{endDate:yyyy-MM-dd}'
                    );

                    EXECUTE format('CREATE INDEX IF NOT EXISTS %I_ts_idx ON %I (timestamp);',
                                   '{partitionName}_ts_idx', '{partitionName}');
                END IF;
            END $$;
        ";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();
    }

    private void BuildFilters(StringBuilder sql, List<NpgsqlParameter> parameters, LogQueryParameters query)
    {
        int idx = parameters.Count;

        void AddFilter(string column, object? value)
        {
            if (value == null) return;
            sql.Append($" AND {column} = @p{idx}");
            parameters.Add(new NpgsqlParameter($"p{idx}", value));
            idx++;
        }

        AddFilter("service_name", query.ServiceName);
        AddFilter("level", query.Level);
        AddFilter("hostname", query.Hostname);
        AddFilter("environment", query.Environment);
        AddFilter("trace_id", query.TraceId);
        AddFilter("span_id", query.SpanId);

        if (query.From.HasValue)
        {
            sql.Append($" AND timestamp >= @p{idx}");
            parameters.Add(new NpgsqlParameter($"p{idx}", query.From.Value));
            idx++;
        }

        if (query.To.HasValue)
        {
            sql.Append($" AND timestamp <= @p{idx}");
            parameters.Add(new NpgsqlParameter($"p{idx}", query.To.Value));
            idx++;
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            if (query.SearchExact is true)
            {
                sql.Append($" AND message = @p{idx}");
                parameters.Add(new NpgsqlParameter($"p{idx}", query.Search));
            }
            else
            {
                sql.Append($" AND message ILIKE @p{idx}");
                parameters.Add(new NpgsqlParameter($"p{idx}", $"%{query.Search}%"));
            }

            idx++;
        }

        if (!string.IsNullOrWhiteSpace(query.Metadata))
        {
            var metadataFilter = JsonSerializer.Deserialize<Dictionary<string, object>>(query.Metadata, _jsonOptions);
            if (metadataFilter != null)
            {
                foreach (var kvp in metadataFilter)
                {
                    sql.Append($" AND metadata ->> @p{idx} = @p{idx + 1}");
                    parameters.Add(new NpgsqlParameter($"p{idx}", kvp.Key));
                    parameters.Add(new NpgsqlParameter($"p{idx + 1}", kvp.Value.ToString() ?? ""));
                    idx += 2;
                }
            }
        }
    }
    private async Task<long> UpsertPatternAsync(
        NpgsqlConnection conn,
        NpgsqlTransaction tx,
        string normalizedMessage,
        ulong patternHash)
    {
        const string sql = @"
INSERT INTO log_patterns (normalized_message, pattern_hash, first_seen, last_seen, occurrence_count)
VALUES (@normalized, @hash, NOW(), NOW(), 1)
ON CONFLICT (pattern_hash)
DO UPDATE SET
    last_seen = NOW(),
    occurrence_count = log_patterns.occurrence_count + 1
RETURNING id;
";

        await using var cmd = new NpgsqlCommand(sql, conn, tx);
        cmd.Parameters.AddWithValue("normalized", normalizedMessage);
        cmd.Parameters.AddWithValue("hash", (long)patternHash); // stored as BIGINT

        return (long)await cmd.ExecuteScalarAsync();
    }

    private LogEntry MapReader(NpgsqlDataReader reader) =>
        new LogEntry
        {
            Timestamp = reader.GetDateTime(0),
            ServiceName = reader.IsDBNull(1) ? null : reader.GetString(1),
            Level = reader.GetString(2),
            Message = reader.GetString(3),
            Metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(reader.GetString(4), _jsonOptions)!,
            TraceId = reader.IsDBNull(5) ? null : reader.GetString(5),
            SpanId = reader.IsDBNull(6) ? null : reader.GetString(6),
            Hostname = reader.IsDBNull(7) ? null : reader.GetString(7),
            Environment = reader.IsDBNull(8) ? null : reader.GetString(8),
        };
}
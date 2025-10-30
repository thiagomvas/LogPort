using System.Data;
using LogPort.Core.Models;
using Npgsql;
using NpgsqlTypes;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using LogPort.Internal.Common.Interface;

namespace LogPort.Data.Postgres;

public class PostgresLogRepository : ILogRepository
{
    private readonly string _connectionString;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly int _partitionLengthInDays;

    public PostgresLogRepository(LogPortConfig config, JsonSerializerOptions? jsonOptions = null)
    {
        _connectionString = config.Postgres.ConnectionString;
        _partitionLengthInDays = config.Postgres.PartitionLength;
        _jsonOptions = jsonOptions ?? new JsonSerializerOptions
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };
    }

    public Task AddLogAsync(LogEntry log) => AddLogsAsync(new[] { log });

    public async Task AddLogsAsync(IEnumerable<LogEntry> logs)
    {
        var logList = logs.ToList();
        if (!logList.Any()) return;

        var uniquePeriods = logList
            .Select(l => l.Timestamp.Date)
            .Distinct();

        foreach (var day in uniquePeriods)
            await EnsurePartitionAsync(day);

        var sqlValues = new List<string>();
        var parameters = new List<NpgsqlParameter>();
        int i = 0;

        foreach (var log in logList)
        {
            sqlValues.Add($"(@ts{i}, @svc{i}, @lvl{i}, @msg{i}, @meta{i}, @trace{i}, @span{i}, @host{i}, @env{i})");
            parameters.AddRange(new[]
            {
                new NpgsqlParameter($"ts{i}", log.Timestamp),
                new NpgsqlParameter($"svc{i}", (object?)log.ServiceName ?? DBNull.Value),
                new NpgsqlParameter($"lvl{i}", log.Level),
                new NpgsqlParameter($"msg{i}", log.Message),
                new NpgsqlParameter($"meta{i}", NpgsqlDbType.Jsonb) { Value = JsonSerializer.Serialize(log.Metadata, _jsonOptions) },
                new NpgsqlParameter($"trace{i}", (object?)log.TraceId ?? DBNull.Value),
                new NpgsqlParameter($"span{i}", (object?)log.SpanId ?? DBNull.Value),
                new NpgsqlParameter($"host{i}", (object?)log.Hostname ?? DBNull.Value),
                new NpgsqlParameter($"env{i}", (object?)log.Environment ?? DBNull.Value),
            });
            i++;
        }

        var sql = "INSERT INTO logs (timestamp, service_name, level, message, metadata, trace_id, span_id, hostname, environment) VALUES "
                  + string.Join(", ", sqlValues);

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters.ToArray());
        await cmd.ExecuteNonQueryAsync();
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

    private async Task EnsurePartitionAsync(DateTime timestamp)
    {
        var startDate = timestamp.Date.AddDays(-((timestamp.Date - DateTime.MinValue.Date).Days % _partitionLengthInDays));
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

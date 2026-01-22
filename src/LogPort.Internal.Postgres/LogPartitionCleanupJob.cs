using LogPort.Internal.Configuration;

using Microsoft.Extensions.Logging;

using Npgsql;

namespace LogPort.Data.Postgres;

public sealed class LogPartitionCleanupJob
{
    private readonly ILogger<LogPartitionCleanupJob> _logger;
    private readonly LogRetentionConfig _config;
    private readonly string _connectionString;

    public LogPartitionCleanupJob(
        ILogger<LogPartitionCleanupJob> logger,
        LogPortConfig config)
    {
        _logger = logger;
        _config = config.Retention;
        _connectionString = config.Postgres.ConnectionString;
    }

    public async Task ExecuteAsync()
    {
        var cutoff = DateTime.UtcNow.Date.AddDays(-_config.RetentionDays);

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT drop_old_log_partitions(@cutoff_date)",
            conn);

        cmd.Parameters.AddWithValue("cutoff_date", cutoff);

        var dropped = (int)(await cmd.ExecuteScalarAsync() ?? 0);

        _logger.LogInformation(
            "Log retention cleanup complete. DroppedPartitions={Dropped}, Cutoff={Cutoff}",
            dropped,
            cutoff);
    }
}


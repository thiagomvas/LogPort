using LogPort.Internal.Abstractions;
using LogPort.Internal.Configuration;

using Microsoft.Extensions.Logging;

using Npgsql;

namespace LogPort.Data.Postgres;

public sealed class LogPartitionCleanupJob : JobBase
{
    private readonly ILogger<LogPartitionCleanupJob> _logger;
    private readonly LogRetentionConfig _config;
    private readonly string _connectionString;
    private readonly string _cron;
    private readonly bool _enabled;

    public LogPartitionCleanupJob(
        ILogger<LogPartitionCleanupJob> logger,
        LogPortConfig config)
    {
        _logger = logger;
        _config = config.Retention;
        _connectionString = config.Postgres.ConnectionString;
        _cron = config.Retention.AutomaticCleanupCron;
        _enabled = config.Retention.EnableAutomaticCleanupJob;
    }

    public override bool Enabled => _enabled;

    public override async Task ExecuteAsync()
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

    public override string Id => JobId;
    public override string Name { get; } = "Log Partition Clean up";
    public override string Description { get; } = "Clears up log partitions that are past the retention period";
    public override string Cron => _cron;

    public static readonly string JobId = "LogPartitionCleanupJob";
}
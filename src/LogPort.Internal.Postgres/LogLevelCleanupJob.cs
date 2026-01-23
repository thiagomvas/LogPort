using LogPort.Core;
using LogPort.Internal.Abstractions;
using LogPort.Internal.Configuration;

using Microsoft.Extensions.Logging;

using Npgsql;

namespace LogPort.Data.Postgres;

public sealed class LogLevelCleanupJob : JobBase
{
    private const int BatchSize = 10_000;

    private readonly ILogger<LogLevelCleanupJob> _logger;
    private readonly LogNormalizer _normalizer;
    private readonly Dictionary<string, TimeSpan> _retentions;
    private readonly string _connectionString;
    private readonly string _cron;
    private readonly bool _enabled;

    public LogLevelCleanupJob(
        ILogger<LogLevelCleanupJob> logger,
        LogPortConfig config, LogNormalizer normalizer)
    {
        _logger = logger;
        _normalizer = normalizer;
        _retentions = config.LevelRetention.Retentions; 
        _connectionString = config.Postgres.ConnectionString;
        _cron = config.Retention.AutomaticCleanupCron;
        _enabled = config.Retention.EnableAutomaticCleanupJob;
    }

    public override bool Enabled => _enabled;

    public override async Task ExecuteAsync()
    {
        foreach (var kvp in _retentions)
        {
            string level = kvp.Key;
            TimeSpan retention = kvp.Value;
            await CleanupLevelAsync(level, retention);
        }
    }

    private async Task CleanupLevelAsync(string level, TimeSpan retention)
    {
        var cutoff = DateTime.UtcNow - retention;
        int totalDeleted = 0;

        level = _normalizer.NormalizeLevel(level);

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        while (true)
        {
            await using var cmd = new NpgsqlCommand(@"
                DELETE FROM logs
                WHERE ctid IN (
                    SELECT ctid
                    FROM logs
                    WHERE level = @level
                      AND timestamp < @cutoff
                    ORDER BY timestamp
                    LIMIT @batch_size
                );", conn);

            cmd.Parameters.AddWithValue("level", level);
            cmd.Parameters.AddWithValue("cutoff", cutoff);
            cmd.Parameters.AddWithValue("batch_size", BatchSize);

            int affected = await cmd.ExecuteNonQueryAsync();
            if (affected == 0) break;

            totalDeleted += affected;
        }

        _logger.LogInformation(
            "Log level cleanup complete. Level={Level}, Cutoff={Cutoff}, Deleted={Deleted}",
            level,
            cutoff,
            totalDeleted);
    }

    public override string Id => JobId;
    public override string Name => "Log Level Cleanup";
    public override string Description =>
        "Deletes old logs based on log level retention rules";
    public override string Cron => _cron;

    public static readonly string JobId = "LogLevelCleanupJob";
}

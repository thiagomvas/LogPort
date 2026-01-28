using LogPort.Internal;

namespace LogPort.Data.Postgres;

public sealed class PartitionManager
{
    private readonly int _partitionLengthDays;

    public PartitionManager(int partitionLengthDays)
    {
        _partitionLengthDays = partitionLengthDays;
    }

    public SqlCommand EnsurePartition(DateTime timestamp)
    {
        var start =
            timestamp.Date.AddDays(
                -((timestamp.Date - DateTime.MinValue.Date).Days % _partitionLengthDays));

        var end = start.AddDays(_partitionLengthDays);
        var name = $"logs_{start:yyyy_MM_dd}_{_partitionLengthDays}d";

        var sql = $@"
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_class WHERE relname = '{name}'
    ) THEN
        EXECUTE format(
            'CREATE TABLE IF NOT EXISTS %I PARTITION OF logs
             FOR VALUES FROM (%L) TO (%L);',
            '{name}', '{start:yyyy-MM-dd}', '{end:yyyy-MM-dd}'
        );

        EXECUTE format(
            'CREATE INDEX IF NOT EXISTS %I_ts_idx ON %I (timestamp);',
            '{name}_ts_idx', '{name}');
    END IF;
END $$;";

        return new SqlCommand(sql);
    }
}

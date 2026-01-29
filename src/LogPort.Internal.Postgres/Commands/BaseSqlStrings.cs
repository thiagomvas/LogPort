namespace LogPort.Data.Postgres.Commands;

public sealed class BaseSqlStrings
{
    public static string SelectLogs =>
        @"
SELECT
    timestamp,
    service_name,
    level,
    message,
    metadata,
    trace_id,
    span_id,
    hostname,
    environment
FROM logs
WHERE 1 = 1
";
}

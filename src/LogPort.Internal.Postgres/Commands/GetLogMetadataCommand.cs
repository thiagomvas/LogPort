using LogPort.Internal;

namespace LogPort.Data.Postgres.Commands;

public static class GetLogMetadataCommand
{
    public static SqlCommand Create(DateTimeOffset? from = null, DateTimeOffset? to = null)
    {
        const string sql = @"
WITH
  filtered_logs AS (
    SELECT *
    FROM logs
    WHERE
      (@from_ts IS NULL OR ""timestamp"" >= @from_ts::timestamptz)
      AND (@to_ts IS NULL OR ""timestamp"" <= @to_ts::timestamptz)
  ),
  lvl_counts AS (
    SELECT jsonb_object_agg(level, count) AS data
    FROM (
      SELECT level, COUNT(*) AS count
      FROM filtered_logs
      WHERE level IS NOT NULL
      GROUP BY level
    ) t
  ),
  svc_counts AS (
    SELECT jsonb_object_agg(service_name, count) AS data
    FROM (
      SELECT service_name, COUNT(*) AS count
      FROM filtered_logs
      WHERE service_name IS NOT NULL
      GROUP BY service_name
    ) t
  ),
  env_counts AS (
    SELECT jsonb_object_agg(environment, count) AS data
    FROM (
      SELECT environment, COUNT(*) AS count
      FROM filtered_logs
      WHERE environment IS NOT NULL
      GROUP BY environment
    ) t
  ),
  host_counts AS (
    SELECT jsonb_object_agg(hostname, count) AS data
    FROM (
      SELECT hostname, COUNT(*) AS count
      FROM filtered_logs
      WHERE hostname IS NOT NULL
      GROUP BY hostname
    ) t
  ),
  distincts AS (
    SELECT
      array_agg(DISTINCT level) AS levels,
      array_agg(DISTINCT environment) AS environments,
      array_agg(DISTINCT service_name) AS services,
      array_agg(DISTINCT hostname) AS hostnames,
      COUNT(*) AS log_count
    FROM filtered_logs
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

        return new SqlCommand(sql, new
        {
            from_ts = from,
            to_ts = to
        });
    }
}

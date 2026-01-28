using System.Text.Json;

using LogPort.Core.Models;
using LogPort.Internal;

namespace LogPort.Data.Postgres.Commands;

public sealed class GetLogsCommand
{
    public static SqlCommand Create(
        LogQueryParameters query,
        JsonSerializerOptions jsonOptions)
    {
        var builder = new SqlBuilder(@"
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
");

        builder.AndEquals("service_name", query.ServiceName);
        builder.AndEquals("level", query.Level);
        builder.AndEquals("hostname", query.Hostname);
        builder.AndEquals("environment", query.Environment);
        builder.AndEquals("trace_id", query.TraceId);
        builder.AndEquals("span_id", query.SpanId);

        builder.AndRange("timestamp", query.From, query.To);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            if (query.SearchExact == true)
                builder.AndEquals("message", query.Search);
            else
                builder.AndLike("message", $"%{query.Search}%");
        }

        if (!string.IsNullOrWhiteSpace(query.Metadata))
        {
            var metadata =
                JsonSerializer.Deserialize<Dictionary<string, object>>(
                    query.Metadata,
                    jsonOptions);

            if (metadata != null)
            {
                foreach (var kvp in metadata)
                {
                    var key = kvp.Key.Replace("'", "''");
                    builder.Append($" AND metadata ->> '{key}' = ");
                    builder.AndEquals("", kvp.Value?.ToString() ?? "");
                }
            }
        }

        var pageSize = query.PageSize ?? 100;
        var page = query.Page ?? 1;
        if (page < 1) page = 1;

        var offset = (page - 1) * pageSize;

        builder.Append(" ORDER BY timestamp DESC");
        builder.Append($" LIMIT {pageSize} OFFSET {offset}");

        return builder.Build();
    }
}

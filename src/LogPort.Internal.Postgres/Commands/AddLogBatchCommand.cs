using System.Text.Json;

using LogPort.Core.Models;
using LogPort.Data.Postgres;
using LogPort.Internal;

public sealed class AddLogBatchCommand
{
    public static SqlCommand Create(IEnumerable<LogEntry> batch)
    {
        var builder = new SqlBuilder("INSERT INTO logs " +
                                     "(timestamp, service_name, level, message, metadata, trace_id, span_id, hostname, environment) VALUES ");

        var first = true;
        int rowIndex = 0;

        foreach (var log in batch)
        {
            if (!first)
                builder.Append(", ");
            first = false;

            var tsParam = $"ts{rowIndex}";
            var serviceParam = $"svc{rowIndex}";
            var levelParam = $"lvl{rowIndex}";
            var msgParam = $"msg{rowIndex}";
            var metaParam = $"meta{rowIndex}";
            var traceParam = $"trace{rowIndex}";
            var spanParam = $"span{rowIndex}";
            var hostParam = $"host{rowIndex}";
            var envParam = $"env{rowIndex}";

            // CAST metadata to jsonb in SQL
            builder.Append($"(@{tsParam}, @{serviceParam}, @{levelParam}, @{msgParam}, @{metaParam}::jsonb, @{traceParam}, @{spanParam}, @{hostParam}, @{envParam})");

            builder.AddParameter(tsParam, log.Timestamp);
            builder.AddParameter(serviceParam, log.ServiceName);
            builder.AddParameter(levelParam, log.Level);
            builder.AddParameter(msgParam, log.Message);
            builder.AddParameter(metaParam, log.Metadata != null ? JsonSerializer.Serialize(log.Metadata) : DBNull.Value);
            builder.AddParameter(traceParam, log.TraceId);
            builder.AddParameter(spanParam, log.SpanId);
            builder.AddParameter(hostParam, log.Hostname);
            builder.AddParameter(envParam, log.Environment);

            rowIndex++;
        }

        return builder.Build();
    }

}
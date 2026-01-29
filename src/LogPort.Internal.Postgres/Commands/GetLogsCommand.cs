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
        var builder = new SqlBuilder(BaseSqlStrings.SelectLogs);

        builder.BuildFilters(query, jsonOptions);

        var pageSize = query.PageSize ?? 100;
        var page = query.Page ?? 1;
        if (page < 1) page = 1;

        var offset = (page - 1) * pageSize;

        builder.Append(" ORDER BY timestamp DESC");
        builder.Append($" LIMIT {pageSize} OFFSET {offset}");

        return builder.Build();
    }
}

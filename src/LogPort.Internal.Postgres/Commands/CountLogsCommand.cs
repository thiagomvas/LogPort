using LogPort.Core.Models;
using LogPort.Internal;

namespace LogPort.Data.Postgres.Commands;

public sealed class CountLogsCommand
{
    public static SqlCommand Create(LogQueryParameters? query = null)
    {
        var builder = new SqlBuilder("SELECT COUNT(*) FROM logs WHERE 1=1");

        if (query != null)
            builder.BuildFilters(query);

        return builder.Build();
    }
}
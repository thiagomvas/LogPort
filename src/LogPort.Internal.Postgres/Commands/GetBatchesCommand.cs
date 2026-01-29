using LogPort.Core.Models;
using LogPort.Internal;

namespace LogPort.Data.Postgres.Commands;

public sealed class GetBatchesCommand
{
    public static SqlCommand Create(
        LogQueryParameters? query = null,
        int batchSize = 100,
        int offset = 0)
    {
        var builder = new SqlBuilder(BaseSqlStrings.SelectLogs);
        
        if (query is not null)
            builder.BuildFilters(query);

        builder.Append(" ORDER BY timestamp ASC");
        builder.Append($" LIMIT {batchSize} OFFSET {offset}");
        
        return builder.Build();
    }
}

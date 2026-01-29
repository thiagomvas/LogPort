using LogPort.Internal;

namespace LogPort.Data.Postgres.Commands;

public sealed class GetLogsByEnvironmentCommand
{
    public static SqlCommand Create(DateTimeOffset? from = null, DateTimeOffset? to = null)
    {
        var builder = new SqlBuilder(@"
SELECT environment, COUNT(*) FROM logs WHERE environment IS NOT NULL
");
        
        if (from.HasValue)
            builder.AndRange("timestamp", from.Value.DateTime, to?.DateTime ?? DateTime.UtcNow);

        builder.Append(" GROUP BY environment");
        
        return builder.Build();
    }
}

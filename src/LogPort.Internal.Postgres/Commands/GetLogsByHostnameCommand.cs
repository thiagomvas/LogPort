using LogPort.Internal;

namespace LogPort.Data.Postgres.Commands;

public sealed class GetLogsByHostnameCommand
{
    public static SqlCommand Create(DateTimeOffset? from = null, DateTimeOffset? to = null)
    {
        var builder = new SqlBuilder(@"
SELECT hostname, COUNT(*) FROM logs WHERE hostname IS NOT NULL
");
        
        if (from.HasValue)
            builder.AndRange("timestamp", from.Value.DateTime, to?.DateTime ?? DateTime.UtcNow);

        builder.Append(" GROUP BY hostname");
        
        return builder.Build();
    }
}

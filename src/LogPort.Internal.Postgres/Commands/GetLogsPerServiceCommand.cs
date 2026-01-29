using LogPort.Internal;

namespace LogPort.Data.Postgres.Commands;

public sealed class GetLogsPerServiceCommand
{
    public static SqlCommand Create(DateTimeOffset? from = null, DateTimeOffset? to = null)
    {
        var builder = new SqlBuilder(@"
SELECT service_name, COUNT(*) FROM logs WHERE service_name IS NOT NULL
");
        
        if (from.HasValue)
            builder.AndRange("timestamp", from.Value.DateTime, to?.DateTime ?? DateTime.UtcNow);

        builder.Append(" GROUP BY service_name");
        
        return builder.Build();
    }
}

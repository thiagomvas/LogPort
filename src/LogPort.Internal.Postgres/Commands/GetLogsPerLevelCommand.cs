using LogPort.Internal;

namespace LogPort.Data.Postgres.Commands;

public sealed class GetLogsPerLevelCommand
{
    public static SqlCommand Create(DateTimeOffset? from = null, DateTimeOffset? to = null)
    {
        var builder = new SqlBuilder(@"
SELECT level, COUNT(*) FROM logs 
");
        
        if (from.HasValue)
            builder.AndRange("timestamp", from.Value.DateTime, to?.DateTime ?? DateTime.UtcNow);

        
        builder.Append(" GROUP BY level");
        
        return builder.Build();
    }
}

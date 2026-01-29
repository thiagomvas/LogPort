using LogPort.Internal;

namespace LogPort.Data.Postgres.Commands;

public sealed class GetEnvironmentsCommand
{
    public static SqlCommand Create()
    {
        var builder = new SqlBuilder(@"
SELECT DISTINCT environment FROM logs WHERE environment IS NOT NULL
");

        return builder.Build();
    }
}

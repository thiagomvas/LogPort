using LogPort.Internal;

namespace LogPort.Data.Postgres.Commands;

public sealed class GetHostnamesCommand
{
    public static SqlCommand Create()
    {
        var builder = new SqlBuilder(@"
SELECT DISTINCT hostname FROM logs WHERE environment IS NOT NULL
");
        return builder.Build();
    }
}

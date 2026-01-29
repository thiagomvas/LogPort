using LogPort.Internal;

namespace LogPort.Data.Postgres.Commands;

public sealed class GetLogLevelsCommand
{
    public static SqlCommand Create()
    {
        var builder = new SqlBuilder(@"
    SELECT DISTINCT level FROM logs
");
        return builder.Build();
    }
}

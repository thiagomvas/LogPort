using LogPort.Internal;

namespace LogPort.Data.Postgres.Commands;

public sealed class GetServicesCommand
{
    public static SqlCommand Create()
    {
        var builder = new SqlBuilder(@"
SELECT DISTINCT service_name FROM logs
");
        return builder.Build();
    }
}

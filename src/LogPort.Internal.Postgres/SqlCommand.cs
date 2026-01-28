namespace LogPort.Data.Postgres;

public sealed class SqlCommand
{
    public string Sql { get; }
    public object? Parameters { get; }

    public SqlCommand(string sql, object? parameters = null)
    {
        Sql = sql;
        Parameters = parameters;
    }
}
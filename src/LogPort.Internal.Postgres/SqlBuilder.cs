using System.Text;

using Dapper;

namespace LogPort.Data.Postgres;

public sealed class SqlBuilder
{
    private readonly StringBuilder _sql;
    private readonly DynamicParameters _parameters = new();
    private int _counter;

    public SqlBuilder(string baseSql)
    {
        _sql = new StringBuilder(baseSql);
    }

    public void AndEquals(string column, object? value)
    {
        if (value == null)
            return;

        var name = Next();
        _sql.Append($" AND {column} = @{name}");
        _parameters.Add(name, value);
    }

    public void AndLike(string column, string value)
    {
        var name = Next();
        _sql.Append($" AND {column} ILIKE @{name}");
        _parameters.Add(name, value);
    }

    public void AndRange(string column, DateTime? from, DateTime? to)
    {
        if (from.HasValue)
        {
            var n = Next();
            _sql.Append($" AND {column} >= @{n}");
            _parameters.Add(n, from.Value);
        }

        if (to.HasValue)
        {
            var n = Next();
            _sql.Append($" AND {column} <= @{n}");
            _parameters.Add(n, to.Value);
        }
    }

    public void Append(string sql) => _sql.Append(sql);

    public SqlCommand Build() =>
        new SqlCommand(_sql.ToString(), _parameters);

    private string Next() => $"p{_counter++}";
}
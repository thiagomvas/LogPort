using System.Data;
using System.Text;

using Dapper;

using LogPort.Internal;

namespace LogPort.Data.Postgres;

/// <summary>
/// Incrementally builds a parameterized SQL query using positional parameters.
/// Designed for use with Dapper and <see cref="SqlCommand"/>.
/// </summary>
/// <remarks>
/// This builder owns the SQL text and the associated <see cref="DynamicParameters"/>.
/// Callers should not mutate parameters after <see cref="Build"/> is called.
/// </remarks>
public sealed class SqlBuilder
{
    private readonly StringBuilder _sql;
    private readonly DynamicParameters _parameters = new();
    private int _counter;

    /// <summary>
    /// Initializes a new <see cref="SqlBuilder"/> with the provided base SQL.
    /// </summary>
    /// <param name="baseSql">The initial SQL fragment to build upon.</param>
    public SqlBuilder(string baseSql)
    {
        _sql = new StringBuilder(baseSql);
    }

    /// <summary>
    /// Appends an equality predicate to the query if the value is not null.
    /// </summary>
    /// <param name="column">The column name to compare.</param>
    /// <param name="value">The value to bind as a parameter.</param>
    public void AndEquals(string column, object? value)
    {
        if (value == null)
            return;

        var name = Next();
        _sql.Append($" AND {column} = @{name}");
        _parameters.Add(name, value);
    }

    /// <summary>
    /// Appends a case-insensitive LIKE predicate to the query.
    /// </summary>
    /// <param name="column">The column name to compare.</param>
    /// <param name="value">The pattern to bind as a parameter.</param>
    public void AndLike(string column, string value)
    {
        var name = Next();
        _sql.Append($" AND {column} ILIKE @{name}");
        _parameters.Add(name, value);
    }

    /// <summary>
    /// Appends a range predicate to the query for the specified column.
    /// </summary>
    /// <param name="column">The column name to filter.</param>
    /// <param name="from">The inclusive lower bound, if specified.</param>
    /// <param name="to">The inclusive upper bound, if specified.</param>
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

    /// <summary>
    /// Appends raw SQL to the current query.
    /// </summary>
    /// <param name="sql">The SQL fragment to append.</param>
    public void Append(string sql) => _sql.Append(sql);

    /// <summary>
    /// Builds the final <see cref="SqlCommand"/> containing the SQL and parameters.
    /// </summary>
    /// <returns>A <see cref="SqlCommand"/> ready for execution.</returns>
    public SqlCommand Build() =>
        new SqlCommand(_sql.ToString(), _parameters);
    
    public void AddParameter(string name, object? value) => _parameters.Add(name, value);

    public void AddJsonbParameter(string name, object? value)
    {
        _parameters.Add(name, value ?? DBNull.Value, DbType.Object);
    }


    private string Next() => $"p{_counter++}";
}

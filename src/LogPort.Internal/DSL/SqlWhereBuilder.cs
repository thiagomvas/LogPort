namespace LogPort.Internal.DSL;

public sealed class SqlWhereBuilder
{
    private int _paramIndex;
    private readonly Dictionary<string, object> _parameters = new();

    public (string sql, IReadOnlyDictionary<string, object> parameters) Build(Expr expr)
    {
        var sql = Visit(expr);
        return (sql, _parameters);
    }

    private string Visit(Expr expr) =>
        expr switch
        {
            BinaryExpr b => VisitBinary(b),
            IdentifierExpr i => i.Name,
            ValueExpr v => AddParam(v.Value),
            _ => throw new NotSupportedException()
        };

    private string VisitBinary(BinaryExpr b)
    {
        var left = Visit(b.Left);
        var right = Visit(b.Right);

        return b.Operator.ToLowerInvariant() switch
        {
            "and" => $"({left} AND {right})",
            "or" => $"({left} OR {right})",
            "contains" => $"{left} LIKE {right}",
            "=" or "!=" or ">" or "<" or ">=" or "<=" =>
                $"{left} {b.Operator} {right}",
            _ => throw new NotSupportedException($"Operator {b.Operator}")
        };
    }

    private string AddParam(object value)
    {
        var name = $"@p{_paramIndex++}";

        if (value is string s)
            value = $"%{s}%";

        _parameters[name] = value;
        return name;
    }
}
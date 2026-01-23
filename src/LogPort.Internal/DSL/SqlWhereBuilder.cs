namespace LogPort.Internal.DSL;

public sealed class SqlWhereBuilder
{
    private int _paramIndex;
    private readonly Dictionary<string, object> _parameters = new();
    private static readonly Dictionary<string, string> ColumnMap = new()
    {
        ["serviceName"] = "service_name",
        ["level"] = "level",
        ["message"] = "message",
        ["hostname"] = "hostname",
        ["environment"] = "environment",
        ["spanId"] = "span_id",
        ["traceId"] = "trace_id",
        ["metadata"] = "metadata",
        ["timestamp"] = "timestamp"
    };


    public (string where, IReadOnlyDictionary<string, object> parameters)
        Build(Expr expr)
    {
        _paramIndex = 0;
        _parameters.Clear();
        return (Visit(expr), _parameters);
    }

    private string Visit(Expr expr) =>
        expr switch
        {
            BinaryExpr b => VisitBinary(b),
            IdentifierExpr i => ColumnMap.TryGetValue(i.Name, out var col) ? col : i.Name,
            ValueExpr v => AddParam(v.Value, v.ContextOperator),
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
            "contains" => $"{left} ILIKE {right}",
            _ => $"{left} {b.Operator} {right}"
        };
    }


    private string AddParam(object value, string? op = null)
    {
        var name = $"@p{_paramIndex++}";

        if (string.Equals(op, "contains", StringComparison.OrdinalIgnoreCase) && value is string s)
            _parameters[name] = $"%{s}%";
        else
            _parameters[name] = value!;

        return name;
    }
}
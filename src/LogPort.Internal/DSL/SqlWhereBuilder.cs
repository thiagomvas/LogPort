namespace LogPort.Internal.DSL;

public sealed class SqlWhereBuilder
{
    private int _paramIndex;
    private readonly Dictionary<string, object> _parameters = new();

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
            _ => $"{left} {b.Operator} {right}"
        };
    }

    private string AddParam(string value)
    {
        var name = $"@p{_paramIndex++}";
        _parameters[name] = $"%{value}%";
        return name;
    }
}
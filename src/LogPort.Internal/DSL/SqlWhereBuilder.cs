using System.Text.Json;

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
            MetadataKeyExpr k => VisitMetadataKey(k),
            MemberExpr m => VisitMember(m),
            BinaryExpr b => VisitBinary(b),
            IdentifierExpr i => ColumnMap.TryGetValue(i.Name, out var col) ? col : i.Name,
            ValueExpr v => AddParam(v.Value, v.ContextOperator),
            _ => throw new NotSupportedException()
        };

    private string VisitMetadataKey(MetadataKeyExpr k)
    {
        return $"(metadata ->> '{k.Key}')";
    }


    private string VisitMember(MemberExpr m)
    {
        if (!IsMetadataChain(m, out var flatKey))
            throw new NotSupportedException("Only metadata member access is supported");

        return $"(metadata ->> '{flatKey}')";
    }

    private bool IsMetadataChain(MemberExpr m, out string flatKey)
    {
        var parts = new Stack<string>();
        Expr current = m;

        while (current is MemberExpr mm)
        {
            parts.Push(mm.Member);
            current = mm.Target;
        }

        if (current is not IdentifierExpr id ||
            !string.Equals(id.Name, "metadata", StringComparison.OrdinalIgnoreCase))
        {
            flatKey = "";
            return false;
        }

        flatKey = string.Join(".", parts);
        return true;
    }


    private string VisitBinary(BinaryExpr b)
    {
        if (IsMetadataBinary(b, out var sql))
            return sql;

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

    private bool IsMetadataBinary(BinaryExpr b, out string sql)
    {
        sql = "";

        if (b.Left is not MemberExpr m)
            return false;

        if (!IsMetadataChain(m, out var flatKey))
            return false;

        var op = b.Operator.ToLowerInvariant();

        switch (op)
        {
            case "=":
            case "!=":
            case ">":
            case "<":
            case ">=":
            case "<=":
                {
                    var param = AddParam(((ValueExpr)b.Right).Value);
                    var cast = op is ">" or "<" or ">=" or "<=" ? "::numeric" : "";
                    sql = $"((metadata ->> '{flatKey}'){cast} {b.Operator} {param})";
                    return true;
                }

            case "contains":
                {
                    var param = AddJsonParam(((ValueExpr)b.Right).Value);
                    sql = $"(metadata @> {param}::jsonb)";
                    return true;
                }

            case "has":
                {
                    sql = $"(metadata ? '{flatKey}')";
                    return true;
                }
        }

        return false;
    }


    private string AddJsonParam(object value)
    {
        var name = $"@p{_paramIndex++}";
        _parameters[name] = JsonSerializer.Serialize(value);
        return name;
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
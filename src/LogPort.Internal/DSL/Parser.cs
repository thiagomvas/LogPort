namespace LogPort.Internal.DSL;

public sealed class Parser
{
    private readonly List<Token> _tokens;
    private int _pos;

    public Parser(IEnumerable<Token> tokens)
    {
        _tokens = tokens.ToList();
    }

    public Expr Parse()
        => ParseOr();

    private Expr ParseOr()
    {
        var expr = ParseAnd();

        while (Match(TokenType.Conditional, "or"))
        {
            var op = Previous().Lexeme;
            var right = ParseAnd();
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    private Expr ParseAnd()
    {
        var expr = ParseComparison();

        while (Match(TokenType.Conditional, "and"))
        {
            var op = Previous().Lexeme;
            var right = ParseComparison();
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    private Expr ParseComparison()
    {
        var left = ParsePrimary();

        if (Match(TokenType.Operator))
        {
            var op = Previous().Lexeme;
            var right = ParsePrimary();
            return new BinaryExpr(left, op, right);
        }

        return left;
    }

    private Expr ParsePrimary()
    {
        if (Match(TokenType.Property))
            return new IdentifierExpr(Previous().Lexeme);

        if (Match(TokenType.Value))
            return new ValueExpr(Previous().Lexeme);

        throw new InvalidOperationException("Unexpected token");
    }

    private bool Match(TokenType type, string? lexeme = null)
    {
        if (IsAtEnd()) return false;

        var token = _tokens[_pos];
        if (token.Type != type) return false;
        if (lexeme != null && !token.Lexeme.Equals(lexeme, StringComparison.OrdinalIgnoreCase))
            return false;

        _pos++;
        return true;
    }

    private Token Previous() => _tokens[_pos - 1];
    private bool IsAtEnd() => _pos >= _tokens.Count;
}
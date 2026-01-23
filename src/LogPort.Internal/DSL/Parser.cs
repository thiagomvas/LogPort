namespace LogPort.Internal.DSL;

public sealed class Parser
{
    private readonly List<Token> _tokens;
    private int _pos;

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    public Expr Parse() => ParseOr();

    private Expr ParseOr()
    {
        var expr = ParseAnd();

        while (Match(TokenType.Conditional, "or"))
            expr = new BinaryExpr(expr, "or", ParseAnd());

        return expr;
    }

    private Expr ParseAnd()
    {
        var expr = ParseComparison();

        while (Match(TokenType.Conditional, "and"))
            expr = new BinaryExpr(expr, "and", ParseComparison());

        return expr;
    }

    private Expr ParseComparison()
    {
        var left = ParsePrimary();

        if (Match(TokenType.Operator))
        {
            var op = Previous().Lexeme;
            var right = ParsePrimary();

            if (right is ValueExpr v)
                right = new ValueExpr(v.Value, op);

            return new BinaryExpr(left, op, right);
        }

        return left;
    }


    private Expr ParsePrimary()
    {
        Expr expr;

        if (Match(TokenType.Property))
            expr = new IdentifierExpr(Previous().Lexeme);
        else if (Match(TokenType.Value))
            expr = new ValueExpr(Previous().Lexeme);
        else
            throw new InvalidOperationException("Unexpected token");

        while (Match(TokenType.Operator, "."))
        {
            if (!Match(TokenType.Value))
                throw new InvalidOperationException("Expected member name");

            expr = new MemberExpr(expr, Previous().Lexeme);
        }

        return expr;
    }


    private bool Match(TokenType type, string? lexeme = null)
    {
        if (IsAtEnd()) return false;

        var t = _tokens[_pos];
        if (t.Type != type) return false;
        if (lexeme != null &&
            !t.Lexeme.Equals(lexeme, StringComparison.OrdinalIgnoreCase))
            return false;

        _pos++;
        return true;
    }

    private Token Previous() => _tokens[_pos - 1];
    private bool IsAtEnd() => _pos >= _tokens.Count;
}
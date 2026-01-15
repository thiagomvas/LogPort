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
            return new BinaryExpr(left, Previous().Lexeme, ParsePrimary());

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
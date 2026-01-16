namespace LogPort.Internal.DSL;

public sealed class QueryCompiler
{
    private readonly Tokenizer _tokenizer = new();
    private readonly SqlWhereBuilder _sqlBuilder = new();
    private readonly List<Token> _tokens = new(32);

    public (string where, IReadOnlyDictionary<string, object> parameters)
        Compile(string query)
    {
        _tokens.Clear();

        _tokenizer.TokenizeInto(query, _tokens);

        var parser = new Parser(_tokens);
        var ast = parser.Parse();

        return _sqlBuilder.Build(ast);
    }
}
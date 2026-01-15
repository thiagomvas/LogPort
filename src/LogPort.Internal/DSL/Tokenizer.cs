using LogPort.Core.Models;
using System.Globalization;
using System.Text;

namespace LogPort.Internal.DSL;

public sealed class Tokenizer
{
    private static readonly HashSet<string> Conditionals =
        new(StringComparer.OrdinalIgnoreCase) { "and", "or", "not" };

    private static readonly HashSet<string> Operators =
        new() { "=", "!=", ">", "<", ">=", "<=", "contains" };

    private readonly HashSet<string> _properties;

    public Tokenizer()
    {
        _properties = typeof(LogEntry)
            .GetProperties()
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public IEnumerable<Token> Tokenize(string query)
    {
        var tokens = new List<Token>();
        var i = 0;

        while (i < query.Length)
        {
            if (char.IsWhiteSpace(query[i]))
            {
                i++;
                continue;
            }

            if (query[i] == '"')
            {
                i++;
                var sb = new StringBuilder();

                while (i < query.Length && query[i] != '"')
                    sb.Append(query[i++]);

                i++; 
                tokens.Add(new Token(TokenType.Value, sb.ToString()));
                continue;
            }

            var op = TryReadOperator(query, ref i);
            if (op != null)
            {
                tokens.Add(new Token(TokenType.Operator, op));
                continue;
            }

            var word = ReadWord(query, ref i);

            if (Conditionals.Contains(word))
            {
                tokens.Add(new Token(TokenType.Conditional, word));
            }
            else if (Operators.Contains(word))
            {
                tokens.Add(new Token(TokenType.Operator, word));
            }
            else if (_properties.Contains(word))
            {
                tokens.Add(new Token(TokenType.Property, word));
            }
            else
            {
                tokens.Add(new Token(TokenType.Value, word));
            }
        }

        return tokens;
    }

    private static string ReadWord(string input, ref int i)
    {
        var start = i;
        while (i < input.Length && !char.IsWhiteSpace(input[i]))
            i++;

        return input[start..i];
    }

    private static string? TryReadOperator(string input, ref int i)
    {
        var remaining = input[i..];

        foreach (var op in new[] { ">=", "<=", "!=", "=", ">", "<" })
        {
            if (remaining.StartsWith(op))
            {
                i += op.Length;
                return op;
            }
        }

        return null;
    }
}

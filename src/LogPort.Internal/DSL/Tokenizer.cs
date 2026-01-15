using LogPort.Core.Models;
using System.Text;

namespace LogPort.Internal.DSL;

public sealed class Tokenizer
{
    private static readonly HashSet<string> Conditionals =
        new(StringComparer.OrdinalIgnoreCase) { "and", "or", "not" };

    private static readonly HashSet<string> Operators =
        new(StringComparer.OrdinalIgnoreCase)
        { "=", "!=", ">", "<", ">=", "<=", "contains" };

    private readonly HashSet<string> _properties;

    public Tokenizer()
    {
        _properties = typeof(LogEntry)
            .GetProperties()
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public void TokenizeInto(string query, List<Token> destination)
    {
        var i = 0;

        while (i < query.Length)
        {
            if (char.IsWhiteSpace(query[i]))
            {
                i++;
                continue;
            }

            // quoted value
            if (query[i] == '"')
            {
                i++;
                var sb = new StringBuilder();

                while (i < query.Length && query[i] != '"')
                    sb.Append(query[i++]);

                i++; // closing "
                destination.Add(new(TokenType.Value, sb.ToString()));
                continue;
            }

            // symbolic operator
            var op = TryReadOperator(query, ref i);
            if (op != null)
            {
                destination.Add(new(TokenType.Operator, op));
                continue;
            }

            // word
            var word = ReadWord(query, ref i);

            if (Conditionals.Contains(word))
                destination.Add(new(TokenType.Conditional, word));
            else if (Operators.Contains(word))
                destination.Add(new(TokenType.Operator, word));
            else if (_properties.Contains(word))
                destination.Add(new(TokenType.Property, word));
            else
                destination.Add(new(TokenType.Value, word));
        }
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
        var span = input.AsSpan(i);

        foreach (var op in new[] { ">=", "<=", "!=", "=", ">", "<" })
        {
            if (span.StartsWith(op))
            {
                i += op.Length;
                return op;
            }
        }

        return null;
    }
}

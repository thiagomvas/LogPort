using LogPort.Internal.DSL;

namespace LogPort.Internal.UnitTests.DSL;


[TestFixture]
public sealed class TokenizerTests
{
    [Test]
    public void Tokenize_QuotedString_IsSingleValue()
    {
        var tokenizer = new Tokenizer();
        var tokens = new List<Token>();

        tokenizer.TokenizeInto(
            "message = \"something failed\"",
            tokens
        );

        Assert.That(tokens.Select(t => t.Type), Is.EqualTo(new[]
        {
            TokenType.Property,
            TokenType.Operator,
            TokenType.Value
        }));

        Assert.That(tokens[2].Lexeme, Is.EqualTo("something failed"));
    }
}

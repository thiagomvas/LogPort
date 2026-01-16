using NUnit.Framework;
using LogPort.Internal.DSL;

namespace LogPort.Internal.UnitTests.DSL;

[TestFixture]
public sealed class QueryCompilerTests
{
    private QueryCompiler _compiler;

    [SetUp]
    public void Setup()
    {
        _compiler = new QueryCompiler();
    }

    [Test]
    public void Compile_SimpleEquality()
    {
        var (where, parameters) = _compiler.Compile(
            "level = Error"
        );

        Assert.That(where, Is.EqualTo("level = @p0"));
        Assert.That(parameters["@p0"], Is.EqualTo("Error"));
    }

    [Test]
    public void Compile_AndCondition()
    {
        var (where, parameters) = _compiler.Compile(
            "level = Error and serviceName = api"
        );

        Assert.That(
            where,
            Is.EqualTo("(level = @p0 AND service_name = @p1)")
        );

        Assert.That(parameters["@p0"], Is.EqualTo("Error"));
        Assert.That(parameters["@p1"], Is.EqualTo("api"));
    }

    [Test]
    public void Compile_ContainsOperator()
    {
        var (where, parameters) = _compiler.Compile(
            "message contains \"failed\""
        );

        Assert.That(where, Is.EqualTo("message LIKE @p0"));
        Assert.That(parameters["@p0"], Is.EqualTo("%failed%"));
    }

    [Test]
    public void Compile_OrHasLowerPrecedenceThanAnd()
    {
        var (where, _) = _compiler.Compile(
            "level = Error or level = Warn and serviceName = api"
        );

        Assert.That(
            where,
            Is.EqualTo("(level = @p0 OR (level = @p1 AND service_name = @p2))")
        );
    }
}
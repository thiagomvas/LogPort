using LogPort.Internal.DSL;

using NUnit.Framework;
namespace LogPort.Internal.UnitTests.DSL;

[TestFixture]
public sealed class SqlWhereBuilderTests
{
    [Test]
    public void Build_MapsPropertyNames()
    {
        var expr = new BinaryExpr(
            new IdentifierExpr("serviceName"),
            "=",
            new ValueExpr("api")
        );

        var builder = new SqlWhereBuilder();
        var (where, parameters) = builder.Build(expr);

        Assert.That(where, Is.EqualTo("service_name = @p0"));
        Assert.That(parameters["@p0"], Is.EqualTo("api"));
    }

    [Test]
    public void Build_ContainsWrapsValue()
    {
        var expr = new BinaryExpr(
            new IdentifierExpr("message"),
            "contains",
            new ValueExpr("error", "contains")
        );

        var builder = new SqlWhereBuilder();
        var (where, parameters) = builder.Build(expr);

        Assert.That(where, Is.EqualTo("message ILIKE @p0"));
        Assert.That(parameters["@p0"], Is.EqualTo("%error%"));
    }
}
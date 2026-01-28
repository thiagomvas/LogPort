using LogPort.Core;
using LogPort.Data.Postgres;
using LogPort.Internal.Abstractions;

using Microsoft.Extensions.Logging;

using NSubstitute;

namespace LogPort.Internal.Postgres.UnitTests;

public sealed class PostgresLogPatternStoreTests
{
    private IDbSessionFactory _sessionFactory = null!;
    private IDbSession _session;
    private LogNormalizer _normalizer = null!;
    private PostgresLogPatternStore _store = null!;

    [SetUp]
    public void SetUp()
    {
        _sessionFactory = Substitute.For<IDbSessionFactory>();
        _session = Substitute.For<IDbSession>();
        _normalizer = new LogNormalizer();

        _sessionFactory.Create().Returns(_session);

        _session.OpenAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _session.BeginTransactionAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _store = new PostgresLogPatternStore(
            "cs",
            _normalizer,
            _sessionFactory,
            Substitute.For<ILogger<PostgresLogPatternStore>>());
    }

    [TearDown]
    public async Task TearDown()
    {
        await _session.DisposeAsync();
    }

    [Test]
    public async Task UpsertAsync_WhenSuccessful_CommitsAndReturnsResult()
    {
        _session.QuerySingleAsync<long>(
                Arg.Any<SqlCommand>(),
                Arg.Any<CancellationToken>())
            .Returns(10L);

        var result = await _store.UpsertAsync(
            "message",
            123UL,
            DateTime.UtcNow);

        Assert.That(result, Is.EqualTo(10L));

        await _session.Received(1).CommitAsync();
    }

    [Test]
    public void UpsertAsync_WhenQueryThrows_ExceptionPropagates()
    {
        _session.QuerySingleAsync<long>(
                Arg.Any<SqlCommand>(),
                Arg.Any<CancellationToken>())
            .Returns<Task<long>>(_ => throw new InvalidOperationException());

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            _store.UpsertAsync(
                "message",
                123UL,
                DateTime.UtcNow));

        _session.DidNotReceive().CommitAsync();
    }

    [Test]
    public void UpsertAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        _session.QuerySingleAsync<long>(
                Arg.Any<SqlCommand>(),
                Arg.Any<CancellationToken>())
            .Returns<Task<long>>(_ => throw new OperationCanceledException());

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Assert.ThrowsAsync<OperationCanceledException>(() =>
            _store.UpsertAsync(
                "message",
                123UL,
                DateTime.UtcNow,
                cancellationToken: cts.Token));

        _session.DidNotReceive().CommitAsync();
    }
}
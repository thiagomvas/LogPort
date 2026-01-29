using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LogPort.Core;
using LogPort.Core.Models;
using LogPort.Data.Postgres;
using LogPort.Internal.Abstractions;
using LogPort.Internal.Configuration;

using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace LogPort.Internal.Postgres.UnitTests;

public sealed class PostgresLogStoreTests
{
    private IDbSessionFactory _sessionFactory = null!;
    private IDbSession _session = null!;
    private ILogPatternStore _patternStore = null!;
    private LogNormalizer _normalizer = null!;
    private PostgresLogStore _store = null!;
    private ILogger<PostgresLogStore> _logger = null!;

    [SetUp]
    public void SetUp()
    {
        _sessionFactory = Substitute.For<IDbSessionFactory>();
        _session = Substitute.For<IDbSession>();
        _patternStore = Substitute.For<ILogPatternStore>();
        _normalizer = new LogNormalizer();
        _logger = Substitute.For<ILogger<PostgresLogStore>>();

        _sessionFactory.Create().Returns(_session);
        _session.OpenAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _session.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _session.CommitAsync().Returns(Task.CompletedTask);

        var config = new LogPortConfig { Postgres = new PostgresConfig() { PartitionLength = 1 } };

        _store = new PostgresLogStore(
            config,
            _sessionFactory,
            _patternStore,
            _normalizer,
            _logger);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _session.DisposeAsync();
    }

    [Test]
    public async Task AddAsync_CallsAddBatchAsyncWithSingleLog()
    {
        var log = new LogEntry { Timestamp = DateTime.UtcNow, Message = "test", Level = "INFO" };
        await _store.AddAsync(log);

        await _session.Received(2).ExecuteAsync(Arg.Any<SqlCommand>(), Arg.Any<CancellationToken>()); // pattern + log
        await _session.Received(1).CommitAsync();
    }

    [Test]
    public async Task AddBatchAsync_WhenLogsEmpty_DoesNothing()
    {
        await _store.AddBatchAsync(Array.Empty<LogEntry>());

        await _session.DidNotReceiveWithAnyArgs().ExecuteAsync(default!, default);
        await _session.DidNotReceive().CommitAsync();
    }

    [Test]
    public async Task AddBatchAsync_InsertsLogsAndCommits()
    {
        var log = new LogEntry { Timestamp = DateTime.UtcNow, Message = "message", Level = "INFO" };
        _patternStore.UpsertAsync(Arg.Any<string>(), Arg.Any<ulong>(), Arg.Any<DateTime>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(1L);

        await _store.AddBatchAsync(new[] { log });

        await _patternStore.Received(1).UpsertAsync(
            Arg.Any<string>(),
            Arg.Any<ulong>(),
            Arg.Any<DateTime>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());

        await _session.Received(2).ExecuteAsync(Arg.Any<SqlCommand>(), Arg.Any<CancellationToken>());
        await _session.Received(1).CommitAsync();
    }

    [Test]
    public void AddBatchAsync_WhenException_Throws()
    {
        var log = new LogEntry { Timestamp = DateTime.UtcNow, Message = "msg", Level = "INFO" };
        _session.ExecuteAsync(Arg.Any<SqlCommand>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException());

        Assert.ThrowsAsync<InvalidOperationException>(() => _store.AddBatchAsync(new[] { log }));
    }

    [Test]
    public async Task GetAsync_ReturnsLogEntries()
    {
        var query = new LogQueryParameters();
        var dto = new LogEntryDto { Timestamp = DateTime.UtcNow, Message = "msg", Level = "INFO" };
        _session.QueryAsync<LogEntryDto>(Arg.Any<SqlCommand>(), Arg.Any<CancellationToken>())
            .Returns(new[] { dto });

        var result = await _store.GetAsync(query);

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Message, Is.EqualTo("msg"));
    }

    [Test]
    public async Task GetBatchesAsync_YieldsBatchesUntilEmpty()
    {
        var dtos1 = new List<LogEntryDto> { new() { Timestamp = DateTime.UtcNow, Message = "1", Level = "INFO" } };
        var dtos2 = new List<LogEntryDto>(); 

        var sessionMock = Substitute.For<IDbSession>();

        sessionMock.QueryAsync<LogEntryDto>(Arg.Any<SqlCommand>(), Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult((IEnumerable<LogEntryDto>)dtos1),
                Task.FromResult((IEnumerable<LogEntryDto>)dtos2)
            );

        _sessionFactory.Create().Returns(sessionMock);

        var batches = new List<IReadOnlyList<LogEntry>>();
        await foreach (var batch in _store.GetBatchesAsync(batchSize: 1))
        {
            batches.Add(batch);
        }

        Assert.That(batches.Count, Is.EqualTo(1));
        Assert.That(batches[0][0].Message, Is.EqualTo("1"));

        await sessionMock.Received(2).QueryAsync<LogEntryDto>(Arg.Any<SqlCommand>(), Arg.Any<CancellationToken>());
    }



    [Test]
    public async Task CountAsync_ExecutesScalar()
    {
        var query = new LogQueryParameters();
        _session.ExecuteScalarAsync<long>(Arg.Any<SqlCommand>(), Arg.Any<CancellationToken>()).Returns(5L);

        var result = await _store.CountAsync(query);

        Assert.That(result, Is.EqualTo(5L));
        await _session.Received(1).ExecuteScalarAsync<long>(Arg.Any<SqlCommand>(), Arg.Any<CancellationToken>());
    }
}

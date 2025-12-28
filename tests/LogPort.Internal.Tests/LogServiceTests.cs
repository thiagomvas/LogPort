using LogPort.Core.Models;
using LogPort.Internal.Abstractions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LogPort.Internal.Tests;

[TestFixture]
public class LogServiceTests
{
    private ILogRepository _repository;
    private ICache _cache;
    private LogPortConfig _config;
    private ILogger<LogService> _logger;
    private LogService _service;

    [SetUp]
    public void Setup()
    {
        _repository = Substitute.For<ILogRepository>();
        _cache = Substitute.For<ICache>();
        _config = new LogPortConfig
        {
            Cache = new LogPortConfig.CacheConfig
            {
                DefaultExpiration = TimeSpan.FromMinutes(5)
            }
        };
        _logger = Substitute.For<ILogger<LogService>>();
        _service = new LogService(_repository, _cache, _config, _logger);
    }

    [Test]
    public async Task GetLogsAsync_UsesCache()
    {
        var parameters = new LogQueryParameters();
        var key = $"{CacheKeys.LogPrefix}{parameters.GetCacheKey()}";
        var expected = new List<LogEntry> { new LogEntry() };

        _cache.GetAsync<IEnumerable<LogEntry>>(key).Returns(expected);

        var result = await _service.GetLogsAsync(parameters);

        Assert.That(result, Is.EqualTo(expected));
        await _repository.DidNotReceive().GetLogsAsync(parameters);
    }

    [Test]
    public async Task GetLogsAsync_FetchesAndCaches()
    {
        var parameters = new LogQueryParameters();
        var key = $"{CacheKeys.LogPrefix}{parameters.GetCacheKey()}";

        _cache.GetAsync<IEnumerable<LogEntry>>(key).Returns((IEnumerable<LogEntry>)null);
        var repoResult = new List<LogEntry> { new LogEntry() };
        _repository.GetLogsAsync(parameters).Returns(repoResult);

        var result = await _service.GetLogsAsync(parameters);

        Assert.That(result, Is.EqualTo(repoResult));
        await _cache.Received(1).SetAsync(key, repoResult, _config.Cache.DefaultExpiration);
    }

    [Test]
    public async Task CountLogsAsync_UsesCache()
    {
        var parameters = new LogQueryParameters();
        var key = $"{CacheKeys.CountPrefix}{parameters.GetCacheKey()}";

        _cache.GetAsync<long?>(key).Returns(42);

        var result = await _service.CountLogsAsync(parameters);

        Assert.That(result, Is.EqualTo(42));
        await _repository.DidNotReceive().CountLogsAsync(parameters);
    }

    [Test]
    public async Task CountLogsAsync_FetchesAndCaches()
    {
        var parameters = new LogQueryParameters();
        var key = $"{CacheKeys.CountPrefix}{parameters.GetCacheKey()}";

        _cache.GetAsync<long?>(key).Returns((long?)null);
        _repository.CountLogsAsync(parameters).Returns(99);

        var result = await _service.CountLogsAsync(parameters);

        Assert.That(result, Is.EqualTo(99L));
        await _cache.Received(1).SetAsync(key, 99L, _config.Cache.DefaultExpiration);
    }

    [Test]
    public async Task GetLogMetadataAsync_UsesCache()
    {
        var key = CacheKeys.BuildLogMetadataCacheKey();
        var expected = new LogMetadata();

        _cache.GetAsync<LogMetadata>(key).Returns(expected);

        var result = await _service.GetLogMetadataAsync();

        Assert.That(result, Is.EqualTo(expected));
        await _repository.DidNotReceive().GetLogMetadataAsync();
    }

    [Test]
    public async Task GetLogMetadataAsync_FetchesAndCaches()
    {
        var key = CacheKeys.BuildLogMetadataCacheKey();

        _cache.GetAsync<LogMetadata>(key).Returns((LogMetadata)null);
        var repoResult = new LogMetadata();
        _repository.GetLogMetadataAsync().Returns(repoResult);

        var result = await _service.GetLogMetadataAsync();

        Assert.That(result, Is.EqualTo(repoResult));
        await _cache.Received(1).SetAsync(key, repoResult, _config.Cache.DefaultExpiration);
    }
}
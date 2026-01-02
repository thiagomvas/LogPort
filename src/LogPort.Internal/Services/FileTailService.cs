using System.Collections.Frozen;
using System.IO;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

using LogPort.Core;
using LogPort.Core.Models;
using LogPort.Internal.Configuration;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LogPort.Internal.Services;

public sealed class FileTailService : BackgroundService
{
    private readonly FrozenDictionary<string, string> _fileToService;
    private readonly LogQueue _queue;
    private readonly ILogger<FileTailService>? _logger;

    

    public FileTailService(LogPortConfig config, LogQueue queue, ILogger<FileTailService>? logger = null)
    {
        _logger = logger;
        _queue = queue;
        _fileToService = config.FileTails
            .Where(f => File.Exists(f.Path))            
            .ToFrozenDictionary(c => c.ServiceName, c => c.Path);
        if (_fileToService.Count > 0)
        {
            _logger?.LogInformation("File tail service started. Tailing {FileCount} files", _fileToService.Count);
        }
    }

    public Task RunAsync(CancellationToken ct = default)
        => RunInternalAsync(ct);

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => RunInternalAsync(stoppingToken);

    private async Task RunInternalAsync(CancellationToken ct)
    {
        var tasks = _fileToService
            .Select(kvp => TailFileAsync(kvp.Key, kvp.Value, ct));

        await Task.WhenAll(tasks);
    }

    private async Task TailFileAsync(string serviceName, string path, CancellationToken ct)
    {
        long lastPosition = File.Exists(path) ? new FileInfo(path).Length : 0;

        while (!ct.IsCancellationRequested)
        {
            try
            {
                if (!File.Exists(path))
                {
                    await Task.Delay(500, ct); // wait for file to exist
                    continue;
                }

                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                stream.Seek(lastPosition, SeekOrigin.Begin);

                using var reader = new StreamReader(stream, Encoding.UTF8);
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var log = new LogEntry() { ServiceName = serviceName, Level = "Info", Message = line, Timestamp = DateTime.UtcNow};
                    _queue.Enqueue(log);
                }

                lastPosition = stream.Position;
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error reading file {Path}", path);
            }

            await Task.Delay(200, ct);
        }
    }
}

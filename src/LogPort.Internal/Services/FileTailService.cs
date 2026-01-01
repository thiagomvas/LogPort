using System.Collections.Frozen;
using System.IO;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using LogPort.Internal.Configuration;

namespace LogPort.Internal.Services;

public class FileTailService
{
    private readonly FrozenDictionary<string, string> _fileToService;

    public readonly Channel<(string Service, string Line)> LinesChannel = Channel.CreateUnbounded<(string, string)>();

    public FileTailService(LogPortConfig config)
    {
        _fileToService = config.FileTails.ToFrozenDictionary(c => c.ServiceName, c => c.Path);
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var tasks = _fileToService.Select(kvp => TailFileAsync(kvp.Key, kvp.Value, cancellationToken));
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
                    await LinesChannel.Writer.WriteAsync((serviceName, line), ct);
                }

                lastPosition = stream.Position;
            }
            catch (IOException)
            {
            }

            await Task.Delay(200, ct);
        }
    }
}

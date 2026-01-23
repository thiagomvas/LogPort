using LogPort.Core.Abstractions;
using LogPort.Core.Models;

namespace LogPort.AspNetCore;

public sealed class HttpRequestEnricher : ILogEnricher
{
    public void Enrich(LogEntry entry)
    {
        var ctx = AspNetLogContext.Current;
        if (ctx == null)
            return;

        var req = ctx.Request;
        entry.Metadata ??= new Dictionary<string, object>();

        entry.Metadata["http.method"] = req.Method;
        entry.Metadata["http.scheme"] = req.Scheme;
        entry.Metadata["http.host"] = req.Host.Value;
        entry.Metadata["http.path"] = req.Path.Value ?? "/";
    }
}

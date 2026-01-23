using LogPort.Core.Abstractions;
using LogPort.Core.Models;

namespace LogPort.AspNetCore;

public sealed class HttpResponseEnricher : ILogEnricher
{
    public void Enrich(LogEntry entry)
    {
        var ctx = AspNetLogContext.Current;
        if (ctx == null)
            return;

        entry.Metadata ??= new Dictionary<string, object>();
        entry.Metadata["http.status_code"] = ctx.Response.StatusCode;
    }
}
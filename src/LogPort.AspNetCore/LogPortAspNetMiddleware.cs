using Microsoft.AspNetCore.Http;

namespace LogPort.AspNetCore;


internal sealed class LogPortAspNetMiddleware
{
    private readonly RequestDelegate _next;

    public LogPortAspNetMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        AspNetLogContext.Current = context;
        try
        {
            await _next(context);
        }
        finally
        {
            AspNetLogContext.Current = null;
        }
    }
}

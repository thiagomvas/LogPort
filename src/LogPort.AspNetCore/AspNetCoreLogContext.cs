using Microsoft.AspNetCore.Http;

namespace LogPort.AspNetCore;

internal static class AspNetLogContext
{
    private static readonly AsyncLocal<HttpContext?> _current = new();

    public static HttpContext? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }
}
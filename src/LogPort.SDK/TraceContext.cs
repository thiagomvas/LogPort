namespace LogPort.SDK;

internal static class TraceContext
{
    public static string? TraceId => System.Diagnostics.Activity.Current?.TraceId.ToString();
    public static string? SpanId => System.Diagnostics.Activity.Current?.SpanId.ToString();

}
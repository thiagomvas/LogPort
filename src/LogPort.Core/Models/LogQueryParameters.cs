using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LogPort.Core.Models;

public class LogQueryParameters
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Level { get; set; }
    public string? Search { get; set; }
    public string? ServiceName { get; set; }
    public string? Hostname { get; set; }
    public string? Environment { get; set; }
    
    public string? TraceId { get; set; }
    public string? SpanId { get; set; }
    public int? Page { get; set; } = 1;
    public bool? SearchExact { get; set; } = true;
    public int? PageSize { get; set; } = 100;
}
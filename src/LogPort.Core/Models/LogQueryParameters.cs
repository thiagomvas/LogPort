namespace LogPort.Core.Models;

public class LogQueryParameters
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Level { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public bool SearchExact { get; set; } = false;
    public int PageSize { get; set; } = 100;
}
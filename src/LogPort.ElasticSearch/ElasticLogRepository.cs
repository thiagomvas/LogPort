using LogPort.Core.Interface;
using LogPort.Core.Models;
using Nest;

namespace LogPort.ElasticSearch;

public class ElasticLogRepository : ILogRepository
{
    private readonly ElasticClient _client;

    public ElasticLogRepository(ElasticClient client)
    {
        _client = client;
    }

    public async Task AddLogAsync(LogEntry log)
    {
        log.Timestamp = log.Timestamp.ToUniversalTime();
        var indexName = $"logs-{log.Timestamp:yyyy.MM.dd}";
        await _client.IndexAsync(log, i => i.Index(indexName));
    }

    public async Task<IEnumerable<LogEntry>> GetLogsAsync(DateTime? from = null, DateTime? to = null, string? level = null)
    {
        var response = await _client.SearchAsync<LogEntry>(s => s
            .Index("logs-*")
            .Query(q =>
            {
                var must = new List<Func<QueryContainerDescriptor<LogEntry>, QueryContainer>>();

                if (from.HasValue || to.HasValue)
                {
                    must.Add(m => m.DateRange(r =>
                    {
                        r.Field(f => f.Timestamp);
                        if (from.HasValue) r.GreaterThanOrEquals(from.Value);
                        if (to.HasValue) r.LessThanOrEquals(to.Value);
                        return r;
                    }));
                }

                if (!string.IsNullOrWhiteSpace(level))
                {
                    must.Add(m => m.Term(t => t.Level, level));
                }

                if (!must.Any()) return q.MatchAll();

                return q.Bool(b => b.Must(must.ToArray()));
            })
            .Sort(srt => srt.Descending(f => f.Timestamp))
            .Size(1000)
        );

        return response.Documents;
    }
}
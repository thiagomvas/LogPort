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

    public async Task<IEnumerable<LogEntry>> GetLogsAsync(LogQueryParameters parameters)
    {
        var response = await _client.SearchAsync<LogEntry>(s => s
            .Index("logs-*")
            .Query(BuildQuery(parameters))
            .Sort(srt => srt.Descending(f => f.Timestamp))
            .From((parameters.Page - 1) * parameters.PageSize)
            .Size(parameters.PageSize)
        );

        return response.Documents;
    }

    public async Task<long> CountLogsAsync(LogQueryParameters parameters)
    {
        var response = await _client.CountAsync<LogEntry>(c => c
            .Index("logs-*")
            .Query(BuildQuery(parameters))
        );

        return response.Count;
    }

    // Reusable query builder used by both Search and Count
    private Func<QueryContainerDescriptor<LogEntry>, QueryContainer> BuildQuery(LogQueryParameters parameters)
    {
        return q =>
        {
            var must = new List<Func<QueryContainerDescriptor<LogEntry>, QueryContainer>>();

            if (parameters.From.HasValue || parameters.To.HasValue)
            {
                must.Add(m => m.DateRange(r =>
                {
                    r.Field(f => f.Timestamp);
                    if (parameters.From.HasValue) r.GreaterThanOrEquals(parameters.From.Value);
                    if (parameters.To.HasValue) r.LessThanOrEquals(parameters.To.Value);
                    return r;
                }));
            }

            if (!string.IsNullOrWhiteSpace(parameters.Level))
            {
                must.Add(m => m.Term(t => t.Level.Suffix("keyword"), parameters.Level));
            }

            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                if (parameters.SearchExact)
                {
                    must.Add(m => m.MultiMatch(mm => mm
                        .Fields(f => f.Field(fe => fe.Message))
                        .Query(parameters.Search)
                        .Type(TextQueryType.BestFields)
                    ));
                }
                else
                {
                    must.Add(m => m.Wildcard(w => w
                        .Field(f => f.Message.Suffix("keyword"))
                        .Value($"{parameters.Search}*")
                    ));

                }

            }

            if (!must.Any()) return q.MatchAll();

            return q.Bool(b => b.Must(must.ToArray()));
        };
    }
}

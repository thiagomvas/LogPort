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

        // Ensure index template for ngram exists
        CreateIndexTemplateIfNotExists();
    }

    private void CreateIndexTemplateIfNotExists()
    {
        var templateExists = _client.Indices.TemplateExists("logs-template").Exists;
        if (!templateExists)
        {
            var createTemplate = _client.Indices.PutTemplate("logs-template", t => t
                .IndexPatterns("logs-*")
                .Settings(s => s
                    .Analysis(a => a
                        .Analyzers(an => an
                            .Custom("ngram_analyzer", ca => ca
                                .Tokenizer("standard")
                                .Filters("lowercase", "ngram_filter")
                            )
                        )
                        .TokenFilters(tf => tf
                            .NGram("ngram_filter", ng => ng
                                .MinGram(2)
                                .MaxGram(20)
                            )
                        )
                    )
                )
                .Mappings(m => m
                    .Map<LogEntry>(mp => mp
                        .AutoMap()
                        .Properties(ps => ps
                            .Text(t => t
                                .Name(n => n.Message)
                                .Fields(f => f
                                    .Keyword(k => k.Name("keyword"))
                                    .Text(tt => tt
                                        .Name("ngram")
                                        .Analyzer("ngram_analyzer")
                                    )
                                )
                            )
                            .Keyword(k => k.Name(n => n.Level))
                            .Keyword(k => k.Name(n => n.ServiceName))
                            .Keyword(k => k.Name(n => n.Environment))
                            .Object<Dictionary<string, object>>(o => o.Name(n => n.Metadata))
                        )
                    )
                )
            );
        }
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
            .Sort(s => s.Descending(f => f.Timestamp).Descending(SortSpecialField.Score))
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

    private Func<QueryContainerDescriptor<LogEntry>, QueryContainer> BuildQuery(LogQueryParameters parameters)
    {
        return q =>
        {
            var must = new List<Func<QueryContainerDescriptor<LogEntry>, QueryContainer>>();

            // Timestamp filter
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
                must.Add(m => m.Term(t => t.Level.Suffix("keyword"), parameters.Level));

            if (!string.IsNullOrWhiteSpace(parameters.ServiceName))
                must.Add(m => m.Term(t => t.ServiceName.Suffix("keyword"), parameters.ServiceName));

            if (!string.IsNullOrWhiteSpace(parameters.Environment))
                must.Add(m => m.Term(t => t.Environment.Suffix("keyword"), parameters.Environment));

            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                if (parameters.SearchExact)
                {
                    must.Add(m => m.MultiMatch(mm => mm
                        .Fields(f => f
                            .Field(fe => fe.Message.Suffix("keyword"))
                            .Field(fe => fe.ServiceName.Suffix("keyword"))
                            .Field(fe => fe.Environment.Suffix("keyword"))
                        )
                        .Query(parameters.Search)
                        .Type(TextQueryType.BestFields)
                    ));
                }
                else
                {
                    must.Add(m => m.MultiMatch(mm => mm
                        .Fields(f => f
                                .Field(fe => fe.Message)       // analyzed field, supports partial
                                .Field(fe => fe.ServiceName)   // analyzed
                                .Field(fe => fe.Environment)   // analyzed
                        )
                        .Query(parameters.Search)
                        .Fuzziness(Fuzziness.Auto)
                        .Type(TextQueryType.BestFields)
                    ));
                }
            }


            if (!must.Any()) return q.MatchAll();

            return q.Bool(b => b.Must(must.ToArray()));
        };
    }
}

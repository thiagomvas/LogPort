using LogPort.Core.Models;
using LogPort.Internal.Abstractions;

using Nest;

namespace LogPort.Internal.ElasticSearch;

public class ElasticLogRepository : ILogRepository
{
    private readonly ElasticClient _client;

    public ElasticLogRepository(ElasticClient client)
    {
        _client = client;

        // Ensure index template exists
        CreateIndexTemplateIfNotExists();
    }

    private void CreateIndexTemplateIfNotExists()
    {
        var templateExists = _client.Indices.TemplateExists("logs-template").Exists;
        if (!templateExists)
        {
            _client.Indices.PutTemplate("logs-template", t => t
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
                            // <-- Store metadata as dynamic for full round-trip
                            .Object<dynamic>(o => o.Name(n => n.Metadata))
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

    public Task AddLogsAsync(IEnumerable<LogEntry> logs)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<LogEntry>> GetLogsAsync(LogQueryParameters parameters)
    {
        var response = await _client.SearchAsync<LogEntry>(s => s
            .Index("logs-*")
            .Query(BuildQuery(parameters))
            .Sort(ss => ss.Descending(f => f.Timestamp).Descending(SortSpecialField.Score))
            .From((parameters.Page - 1) * parameters.PageSize)
            .Size(parameters.PageSize)
        );

        return response.Documents;
    }

    public IAsyncEnumerable<IReadOnlyList<LogEntry>> GetBatchesAsync(LogQueryParameters parameters, int batchSize)
    {
        throw new NotImplementedException();
    }

    public async Task<long> CountLogsAsync(LogQueryParameters parameters)
    {
        var response = await _client.CountAsync<LogEntry>(c => c
            .Index("logs-*")
            .Query(BuildQuery(parameters))
        );

        return response.Count;
    }

    public Task<LogMetadata> GetLogMetadataAsync(DateTimeOffset? from = null, DateTimeOffset? to = null)
    {
        throw new NotImplementedException();
    }

    public Task<LogMetadata> GetLogMetadataAsync()
    {
        throw new NotImplementedException();
    }

    public Task<LogPattern?> GetPatternByHashAsync(string patternHash)
    {
        throw new NotImplementedException();
    }

    public Task<long> CreatePatternAsync(string normalizedMessage, string patternHash, string level)
    {
        throw new NotImplementedException();
    }

    public Task<long> GetOrCreatePatternAsync(string normalizedMessage, string patternHash, DateTime timestamp, string level)
    {
        throw new NotImplementedException();
    }

    public Task UpdatePatternMessageAsync(long patternId, string normalizedMessage)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<LogPattern>> GetPatternsAsync(int limit = 100, int offset = 0)
    {
        throw new NotImplementedException();
    }

    public Task DeletePatternAsync(long patternId)
    {
        throw new NotImplementedException();
    }

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
                must.Add(m => m.Term(t => t.Level.Suffix("keyword"), parameters.Level));

            if (!string.IsNullOrWhiteSpace(parameters.ServiceName))
                must.Add(m => m.Term(t => t.ServiceName.Suffix("keyword"), parameters.ServiceName));

            if (!string.IsNullOrWhiteSpace(parameters.Environment))
                must.Add(m => m.Term(t => t.Environment.Suffix("keyword"), parameters.Environment));

            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                if (parameters.SearchExact is true)
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
                                .Field(fe => fe.Message)
                                .Field(fe => fe.ServiceName)
                                .Field(fe => fe.Environment)
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
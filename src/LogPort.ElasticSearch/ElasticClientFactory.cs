using System.Collections.Specialized;
using LogPort.Core.Models;
using Nest;

namespace LogPort.ElasticSearch;

public static class ElasticClientFactory
{
    public static ElasticClient Create(LogPortConfig config)
    {
        var settings = new ConnectionSettings(new Uri(config.ElasticUri))
            .DefaultIndex(config.DefaultIndex)
            .DefaultFieldNameInferrer(p => p);

        if (!string.IsNullOrWhiteSpace(config.ElasticUsername) && !string.IsNullOrWhiteSpace(config.ElasticPassword))
        {
            settings = settings.BasicAuthentication(config.ElasticUsername, config.ElasticPassword);
        }

        return new ElasticClient(settings);
    }
}
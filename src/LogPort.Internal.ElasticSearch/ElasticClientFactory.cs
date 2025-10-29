using System.Collections.Specialized;
using LogPort.Core.Models;
using Nest;

namespace LogPort.Internal.ElasticSearch;

public static class ElasticClientFactory
{
    public static ElasticClient Create(LogPortConfig config)
    {
        var settings = new ConnectionSettings(new Uri(config.Elastic.Uri))
            .DefaultIndex(config.Elastic.DefaultIndex)
            .DefaultFieldNameInferrer(p => p);

        if (!string.IsNullOrWhiteSpace(config.Elastic.Username) && !string.IsNullOrWhiteSpace(config.Elastic.Password))
        {
            settings = settings.BasicAuthentication(config.Elastic.Username, config.Elastic.Password);
        }

        return new ElasticClient(settings);
    }
}
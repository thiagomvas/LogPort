using Nest;

namespace LogPort.ElasticSearch;

public static class ElasticClientFactory
{
    public static ElasticClient Create(string uri = "http://localhost:9200", string defaultIndex = "logs")
    {
        var settings = new ConnectionSettings(new Uri(uri))
            .DefaultIndex(defaultIndex);

        return new ElasticClient(settings);
    }
}
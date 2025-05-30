namespace RemoteEntity.StatsSink.ElasticSearch;

public record ElasticsearchStatsSinkOptions
{
    public TimeSpan FlushInterval { get; init; }
    public string StatsIndexName { get; init; } = "remoteentity_stats";
}
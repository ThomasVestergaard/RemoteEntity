namespace RemoteEntity.StatsSink.ElasticSearch;

public interface IElasticsearchStatsRepository
{
    Task WriteBatch(IEnumerable<ElasticsearchStatEntry> entries);
}
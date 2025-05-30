using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging;

namespace RemoteEntity.StatsSink.ElasticSearch;

public class ElasticsearchStatsRepository : IElasticsearchStatsRepository
{
    
    private readonly ElasticsearchClient client;
    private readonly ElasticsearchStatsSinkOptions options;
    private readonly ILogger<ElasticsearchStatsRepository> logger;

    public ElasticsearchStatsRepository(ElasticsearchClient client, ElasticsearchStatsSinkOptions options, ILogger<ElasticsearchStatsRepository> logger)
    {
        this.client = client;
        this.options = options;
        this.logger = logger;
    }

    public async Task WriteBatch(IEnumerable<ElasticsearchStatEntry> entries)
    {
        if (!entries.Any())
            return;
        
        var response = await client.BulkAsync(s => s.CreateMany(entries, (descriptor, dto) =>
            descriptor.Index(options.StatsIndexName)
                .Id(new Id($"{dto.AssemblyName.ToLower()}-{dto.StatTypeName.ToLower()}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"))));

        if (response.Errors)
        {
            logger.LogWarning("Failed to write stat entries to elasticsearch: {Error}", response.DebugInformation);
        }
        logger.LogDebug("Flushed {Count} remote entity stats to elasticsearch", entries.Count());
    }

   
}
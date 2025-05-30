using System.Reflection;
using Microsoft.Extensions.Logging;
using RemoteEntity.Stats;
using Timer = System.Timers.Timer;

namespace RemoteEntity.StatsSink.ElasticSearch;

public class ElasticSearchStatsSink : IRemoteEntityStatsSink
{
    private readonly string AssemblyName;
    private StatsCache cache = new();
    private Timer flushTimer;
    private readonly IElasticsearchStatsRepository elasticRepository;
    private readonly ElasticsearchStatsSinkOptions options;
    private readonly ILogger<ElasticSearchStatsSink> logger;

    public ElasticSearchStatsSink(IElasticsearchStatsRepository elasticRepository, ElasticsearchStatsSinkOptions options, ILogger<ElasticSearchStatsSink> logger)
    {
        this.elasticRepository = elasticRepository;
        this.options = options;
        this.logger = logger;
        
        var ea = Assembly.GetEntryAssembly()?.FullName;
        AssemblyName = "Unknown";
        if (ea != null)
        {
            var parts = ea.Split(',');
            if (parts.Length > 1)
            {
                AssemblyName = parts[0];
            }
        }
    }

    public void ProcessStats(IEnumerable<StatEntry> stats)
    {
        cache.RegisterStats(stats);
    }

    public void Start()
    {
        flushTimer = new System.Timers.Timer(options.FlushInterval);
        flushTimer.Elapsed += (_, _) =>
        {
            FlushToElastic();
        };
        flushTimer.Start();
    }
    
    private void FlushToElastic()
    {
        logger.LogTrace("Flushing remote entity stats to elasticsearch");

        var stats = cache.Stats;
        if (stats.Keys.Count == 0) return;
        
        var flattenedEntries = new List<ElasticsearchStatEntry>();
        foreach (var stat in stats)
        {
            foreach (var valueKey in stat.Value.Keys)
            {
                flattenedEntries.Add(new ElasticsearchStatEntry
                {
                    StatKey = valueKey,
                    StatValue = stat.Value[valueKey],
                    StatTypeId = (int)stat.Key,
                    StatTypeName = stat.Key.ToString(),
                    AssemblyName = AssemblyName
                });
            }
        }
        elasticRepository.WriteBatch(flattenedEntries);
        cache.Clear();
    }
}
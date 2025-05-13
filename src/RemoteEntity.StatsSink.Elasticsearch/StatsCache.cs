using RemoteEntity.Stats;

namespace RemoteEntity.StatsSink.Elasticsearch;

public class StatsCache : IStatsCache
{
    private object lockObject = new object();
    private Dictionary<StatTypeEnum, Dictionary<string, long>> cache = new();
    private List<IRemoteEntityStatsSink> sinks = new();
    
    private void EnsureTypeAndKeyExists(StatTypeEnum type, string keyName)
    {
        if (!cache.ContainsKey(type))
            cache.Add(type, new Dictionary<string, long>());
            
        if (!cache[type].ContainsKey(keyName))
            cache[type].Add(keyName, 0);
    }
    
    public void RegisterPublish(StatTypeEnum type, string keyName)
    {
        lock (lockObject)
        {
            EnsureTypeAndKeyExists(type, keyName);
            cache[type][keyName]++;
        }
    }

    public void RegisterPublish(StatTypeEnum type, string keyName, long keyValue)
    {
        lock (lockObject)
        {
            EnsureTypeAndKeyExists(type, keyName);
            cache[type][keyName] += keyValue;
        }
    }

    public void RegisterStatSink(IRemoteEntityStatsSink sink)
    {
        sinks.Add(sink);
    }

    public void FlushToSinks()
    {
        lock (lockObject)
        {
            List<StatEntry> flattened = cache
                .SelectMany(outer => outer.Value.Select(inner => new StatEntry
                {
                    StatType = outer.Key,
                    StatKey = inner.Key,
                    StatValue = inner.Value
                }))
                .ToList();
            
            foreach (var sink in sinks)
            {
                // TODO: Flush
                //sink.Flush(flattened);
            }
            cache.Clear();
        }
    }
}
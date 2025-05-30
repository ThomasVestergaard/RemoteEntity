using RemoteEntity.Stats;

namespace RemoteEntity.StatsSink.ElasticSearch;

internal class StatsCache
{
    private object lockObject = new object();
    private Dictionary<StatTypeEnum, Dictionary<string, long>> cache = new();

    public Dictionary<StatTypeEnum, Dictionary<string, long>> Stats
    {
        get
        {
            lock (lockObject)
            {
                return cache.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new Dictionary<string, long>(kvp.Value)
                );
            }
        }
    }

    public void Clear()
    {
        lock (lockObject)
        {
            cache.Clear();
        }
    }
    
    private void EnsureTypeAndKeyExists(StatTypeEnum type, string keyName)
    {
        if (!cache.ContainsKey(type))
            cache.Add(type, new Dictionary<string, long>());
            
        if (!cache[type].ContainsKey(keyName))
            cache[type].Add(keyName, 0);
    }

    private void RegisterStatUnsafe(StatEntry entry)
    {
        EnsureTypeAndKeyExists(entry.StatType, entry.StatKey);
        cache[entry.StatType][entry.StatKey] += entry.StatValue;
    }

    public void RegisterStat(StatEntry entry)
    {
        lock (lockObject)
        {
            RegisterStatUnsafe(entry);
        }
    }
    
    public void RegisterStats(IEnumerable<StatEntry> entries)
    {
        lock (lockObject)
        {
            foreach (var entry in entries)
                RegisterStat(entry);
        }
    }
}
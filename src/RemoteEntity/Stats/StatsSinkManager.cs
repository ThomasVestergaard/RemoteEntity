using System.Collections.Generic;

namespace RemoteEntity.Stats;

public class StatsSinkManager(IEnumerable<IRemoteEntityStatsSink> statsSinks) : IStatsSinkManager
{
    public void RegisterPublish(string entityId, string entityTypeName, long byteSize)
    {
        var stats = new List<StatEntry>();
        stats.Add(new StatEntry
        {
            StatType = StatTypeEnum.PublishCountByEntityId,
            StatKey = entityId,
            StatValue = 1
        });
        
        stats.Add(new StatEntry
        {
            StatType = StatTypeEnum.PublishCountByEntityType,
            StatKey = entityTypeName,
            StatValue = 1
        });
        
        stats.Add(new StatEntry
        {
            StatType = StatTypeEnum.PublishSizeByEntityId,
            StatKey = entityId,
            StatValue = byteSize
        });
        
        foreach (var sink in statsSinks)
            sink.ProcessStats(stats);
    }
}
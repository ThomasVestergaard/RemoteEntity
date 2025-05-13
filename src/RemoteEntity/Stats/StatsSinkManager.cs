using System.Collections.Generic;

namespace RemoteEntity.Stats;

public class StatsSinkManager(IEnumerable<IRemoteEntityStatsSink> statsSinks) : IStatsSinkManager
{
    public void RegisterPublish(StatTypeEnum type, string keyName)
    {
        foreach (var sink in statsSinks)
            sink.RegisterPublish(type, keyName);
    }

    public void RegisterPublish(StatTypeEnum type, string keyName, long keyValue)
    {
        foreach (var sink in statsSinks)
            sink.RegisterPublish(type, keyName, keyValue);
    }

    
    public void RegisterPublish(string entityId, string entityDtypeName, long byteSize)
    {
       // TODO: Map to sink methods 
    }
}
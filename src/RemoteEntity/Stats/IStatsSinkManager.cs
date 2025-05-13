namespace RemoteEntity.Stats;

public interface IStatsSinkManager
{
    void RegisterPublish(string entityId, string entityDtypeName, long byteSize);
}
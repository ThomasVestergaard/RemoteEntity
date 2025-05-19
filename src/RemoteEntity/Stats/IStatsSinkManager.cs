namespace RemoteEntity.Stats;

public interface IStatsSinkManager
{
    void RegisterPublish(string entityId, string entityTypeName, long byteSize);
    void Start();
}
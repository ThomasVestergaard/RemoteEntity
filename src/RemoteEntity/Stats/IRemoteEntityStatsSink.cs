namespace RemoteEntity.Stats;

public interface IRemoteEntityStatsSink
{
    void RegisterPublish(StatTypeEnum type, string keyName);
    void RegisterPublish(StatTypeEnum type, string keyName, long keyValue);
    
}
namespace RemoteEntity.Stats;

public interface IStatsCache
{
    void RegisterPublish(StatTypeEnum type, string keyName);
    void RegisterPublish(StatTypeEnum type, string keyName, long keyValue);
    void RegisterStatSink(IRemoteEntityStatsSink sink);
    void FlushToSinks();

}
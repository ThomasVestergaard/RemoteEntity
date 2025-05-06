using StackExchange.Redis;

namespace RemoteEntity.Redis;

public interface IRedisConnection
{
    IConnectionMultiplexer Multiplexer { get; }
    void Start();
}
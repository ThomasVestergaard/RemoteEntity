using StackExchange.Redis;

namespace RemoteEntity.Redis;

public class RedisConnection(RedisConnectionOptions options) : IRedisConnection
{
    public IConnectionMultiplexer Multiplexer { get; private set; }

    public void Start()
    {
        Multiplexer = ConnectionMultiplexer.Connect(options.RedisHostName);
    }
}

namespace RemoteEntity.Redis;

public record RedisConnectionOptions
{
    public string RedisHostName { get; init; }
}
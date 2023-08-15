using Microsoft.Extensions.Logging.Abstractions;
using RemoteEntity.Redis;
using StackExchange.Redis;

namespace RemoteEntity.Tests;

public class Producer
{
    public static void Execute()
    {
        var redis = ConnectionMultiplexer.Connect("localhost");
        
        var redisEntityStorage = new RedisEntityStorage(redis);
        var redisEntityPubSub = new RedisEntityPubSub(redis);
        var entityHive = new EntityHive(redisEntityStorage, redisEntityPubSub, NullLogger.Instance);
        
        //redisEntityStorage.
        var obj = new SomeValueObject
        {
            SomeNumber = 456.55m,
            SomeText = "Update"
        };
        
        entityHive.PublishEntity(obj, "ObjectIdentifier");
    }
}
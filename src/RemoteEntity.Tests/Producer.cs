using Microsoft.Extensions.Logging.Abstractions;
using RemoteEntity.Redis;
using StackExchange.Redis;

namespace RemoteEntity.Tests;

public class Producer
{
    public static void Execute()
    {
        var redis = ConnectionMultiplexer.Connect("localhost");
        
        var redisEntityStorage = new RedisEntityStorage(redis, NullLogger<RedisEntityStorage>.Instance);
        var redisEntityPubSub = new RedisEntityPubSub(redis, NullLogger<RedisEntityPubSub>.Instance);
        var entityHive = new EntityHive(redisEntityStorage, redisEntityPubSub, NullLogger<EntityHive>.Instance);
        
        //redisEntityStorage.
        var obj = new SomeValueObject
        {
            SomeNumber = 456.55m,
            SomeText = "Update"
        };
        
        entityHive.PublishEntity(obj, "ObjectIdentifier");
    }
}
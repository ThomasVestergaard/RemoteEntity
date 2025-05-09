using Microsoft.Extensions.Logging.Abstractions;
using RemoteEntity.Redis;

namespace RemoteEntity.Tests;

public class Producer
{
    public static void Execute()
    {
        var redisConfig = new RedisConnectionOptions
        {
            RedisHostName = "localhost",
        };
        var redisConnection = new RedisConnection(redisConfig);
        
        var redisEntityStorage = new RedisEntityStorage(redisConnection, NullLogger<RedisEntityStorage>.Instance);
        var redisEntityPubSub = new RedisEntityPubSub(redisConnection, NullLogger<RedisEntityPubSub>.Instance);
        var entityHive = new EntityHive(redisEntityStorage, redisEntityPubSub, new HiveOptions(), new DuplicateDetector(), NullLogger<EntityHive>.Instance);
        
        //redisEntityStorage.
        var obj = new SomeValueObject
        {
            SomeNumber = 456.55m,
            SomeText = "Update"
        };
        
        entityHive.PublishEntity(obj, "ObjectIdentifier");
    }
}
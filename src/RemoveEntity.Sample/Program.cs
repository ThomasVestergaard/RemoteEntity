using Microsoft.Extensions.Logging.Abstractions;
using RemoteEntity;
using RemoteEntity.Redis;
using StackExchange.Redis;

namespace RemoveEntity.Sample;

public static class Program
{
    public static void Main(string[] args)
    {
        
        var redis = ConnectionMultiplexer.Connect("localhost");

        var redisEntityStorage = new RedisEntityStorage(redis, NullLogger<RedisEntityStorage>.Instance);
        var redisEntityPubSub = new RedisEntityPubSub(redis, NullLogger<RedisEntityPubSub>.Instance);
        var entityHive = new EntityHive(redisEntityStorage, redisEntityPubSub, NullLogger<EntityHive>.Instance);

        var obj = new SomeValueObject
        {
            SomeNumber = 456.55m,
            SomeText = "Update"
        };
        
        entityHive.PublishEntity(obj, "ObjectIdentifier");
        
        
        var keys = redisEntityStorage.GetKeys();

        var subsub = entityHive.GetEntity<SomeValueObject>(keys.First());
        
        
        
        int derp = 0;
        return;
        Task.Factory.StartNew(Consumer.Execute, TaskCreationOptions.LongRunning);
        Thread.Sleep(500);

        Producer.Execute();
    }
}
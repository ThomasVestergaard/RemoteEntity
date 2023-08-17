using System;
using Microsoft.Extensions.Logging.Abstractions;
using RemoteEntity.Redis;
using StackExchange.Redis;

namespace RemoteEntity.Tests;

public class Consumer
{
    public void Initialize()
    {
        var redis = ConnectionMultiplexer.Connect("localhost");
        
        var redisEntityStorage = new RedisEntityStorage(redis);
        var redisEntityPubSub = new RedisEntityPubSub(redis, NullLogger<RedisEntityPubSub>.Instance);
        var entityHive = new EntityHive(redisEntityStorage, redisEntityPubSub, NullLogger<EntityHive>.Instance);
        
        var observable = entityHive.SubscribeToEntity<SomeValueObject>("ObjectIdentifier", entity =>
        {
            // Handle updates as the come
            Console.WriteLine($"Update received. Value is: {entity.SomeText}");
        });
    }
}
using Microsoft.Extensions.Logging.Abstractions;
using RemoteEntity;
using RemoteEntity.Redis;
using StackExchange.Redis;

namespace RemoveEntity.Sample;

public static class Consumer
{
    public static void Execute()
    {
        var redis = ConnectionMultiplexer.Connect("localhost");
        
        var redisEntityStorage = new RedisEntityStorage(redis, NullLogger<RedisEntityStorage>.Instance);
        var redisEntityPubSub = new RedisEntityPubSub(redis, NullLogger<RedisEntityPubSub>.Instance);
        var entityHive = new EntityHive(redisEntityStorage, redisEntityPubSub, NullLogger<EntityHive>.Instance);
        
        var observable = entityHive.SubscribeToEntity<SomeValueObject>("ObjectIdentifier", entity =>
        {
            // Handle updates as the come
            Console.WriteLine($"CONSUMER: Update received. Value is: '{entity.SomeText}'");
        });

        if (observable.Value != null)
        {
            Console.WriteLine($"Entity values when subscribed: '{observable.Value.SomeText}'");
        }
        else
        {
            Console.WriteLine("Entity value when subscribed: null");
        } 
        
        var observable2 = entityHive.SubscribeToEntity<SomeValueObject>("ObjectIdentifier", entity =>
        {
            // Handle updates as the come
            Console.WriteLine($"CONSUMER2: Update received. Value is: '{entity.SomeText}'");
        });
            
    }
}
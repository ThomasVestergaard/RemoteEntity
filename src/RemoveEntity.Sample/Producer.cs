using Microsoft.Extensions.Logging.Abstractions;
using RemoteEntity;
using RemoteEntity.Redis;
using StackExchange.Redis;

namespace RemoveEntity.Sample;

public static class Producer
{
    public static void Execute()
    {
        var redis = ConnectionMultiplexer.Connect("localhost");

        var redisEntityStorage = new RedisEntityStorage(redis);
        var redisEntityPubSub = new RedisEntityPubSub(redis);
        var entityHive = new EntityHive(redisEntityStorage, redisEntityPubSub, NullLogger.Instance);

        var obj = new SomeValueObject
        {
            SomeNumber = 456.55m,
            SomeText = "Update"
        };

        for(int i=0; i<10; i++)
        {
            // Publish 10 messages
            obj.SomeText = $"Update no {i}";
            Console.WriteLine($"PRODUCER: Publishing: '{obj.SomeText}'");

            try
            {
                entityHive.PublishEntity(obj, "ObjectIdentifier");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            Thread.Sleep(1000);
        }

    }
}
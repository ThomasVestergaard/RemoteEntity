using System;
using System.Threading;
using Microsoft.Extensions.Logging.Abstractions;
using RemoteEntity.Redis;
using ServiceStack.Redis;

namespace RemoteEntity.Samples.Producer
{
    class Program
    {
        static void Main(string[] args)
        {
            var redisClientManager = new PooledRedisClientManager("redis://localhost:6379");
            var redisEntityStorage = new RedisEntityStorage(redisClientManager, NullLogger.Instance);
            var redisEntityPubSub = new RedisEntityPubSub(redisClientManager, NullLogger.Instance);
            var entityHive = new EntityHive(redisEntityStorage, redisEntityPubSub, NullLogger.Instance);

            var obj = new SomeValueObject
            {
                SomeNumber = 456.55m,
                SomeText = "Update"
            };

            for(int i=0; i<50; i++)
            {
                // Publish 50 messages
                obj.SomeText = $"Update no {i}";
                Console.WriteLine(obj.SomeText);
                
                entityHive.PublishEntity(obj, "ObjectIdentifier");

                Thread.Sleep(1500);
            }

        }
    }
}

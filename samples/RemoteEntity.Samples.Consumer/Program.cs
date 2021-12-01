using System;
using System.Threading;
using Microsoft.Extensions.Logging.Abstractions;
using RemoteEntity.Redis;
using ServiceStack.Redis;

namespace RemoteEntity.Samples.Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            var redisClientManager = new PooledRedisClientManager("redis://localhost:6379");
            var redisEntityStorage = new RedisEntityStorage(redisClientManager, NullLogger.Instance);
            var redisEntityPubSub = new RedisEntityPubSub(redisClientManager, NullLogger.Instance);
            var entityHive = new EntityHive(redisEntityStorage, redisEntityPubSub, NullLogger.Instance);


            var observable = entityHive.SubscribeToEntity<SomeValueObject>("ObjectIdentifier", entity =>
            {
                // Handle updates as the come
                Console.WriteLine($"Update received. Value is: {entity.SomeText}");

            });


            while (true)
            {
                // Check the latest value whenever you want
                if (observable.Value != null) // Remember to check for null. The object might not exist.
                {
                    Console.WriteLine($"Entity publish time was: {observable.PublishTime.ToString("yyyy-MM-dd hh:mm:ss")}. Current value is : {observable.Value.SomeText}");
                }

                Thread.Sleep(500);
            }


        }
    }
}

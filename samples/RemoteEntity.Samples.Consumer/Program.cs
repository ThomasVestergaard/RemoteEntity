using System;
using System.Threading;
using BeetleX.Redis;
using Microsoft.Extensions.Logging.Abstractions;
using RemoteEntity.Redis;

namespace RemoteEntity.Samples.Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            var redisDb = new RedisDB();
            redisDb.DataFormater = new JsonFormater();
            redisDb.Host.AddWriteHost("localhost");

            var redisEntityStorage = new RedisEntityStorage(redisDb);
            var redisEntityPubSub = new RedisEntityPubSub(redisDb);
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

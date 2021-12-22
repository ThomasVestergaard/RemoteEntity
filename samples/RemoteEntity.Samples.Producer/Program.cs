using System;
using System.Threading;
using BeetleX.Redis;
using Microsoft.Extensions.Logging.Abstractions;
using RemoteEntity.Redis;

namespace RemoteEntity.Samples.Producer
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

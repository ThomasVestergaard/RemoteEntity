﻿using System;
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
            var redisClient = new RedisClient("redis://localhost:6379");
            var redisEntityStorage = new RedisEntityStorage(redisClient, NullLogger.Instance);
            var redisEntityPubSub = new RedisEntityPubSub(redisClient, NullLogger.Instance);
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
                    Console.WriteLine($"Current value is : {observable.Value.SomeText}");
                }

                Thread.Sleep(500);
            }


        }
    }
}
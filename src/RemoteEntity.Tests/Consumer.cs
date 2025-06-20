﻿using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RemoteEntity.Redis;
using RemoteEntity.Stats;
using StackExchange.Redis;

namespace RemoteEntity.Tests;

public class Consumer
{
    public void Initialize()
    {
        var redisConfig = new RedisConnectionOptions
        {
            RedisHostName = "localhost",
        };
        var redisConnection = new RedisConnection(redisConfig);
        
        var redisEntityStorage = new RedisEntityStorage(redisConnection, NullLogger<RedisEntityStorage>.Instance);
        var redisEntityPubSub = new RedisEntityPubSub(redisConnection, NullLogger<RedisEntityPubSub>.Instance);
        var statsSinkManager = new StatsSinkManager(new List<IRemoteEntityStatsSink>());
        var entityHive = new EntityHive(redisEntityStorage, redisEntityPubSub, new HiveOptions(), new DuplicateDetector(), NullLogger<EntityHive>.Instance, statsSinkManager);
        
        var observable = entityHive.SubscribeToEntity<SomeValueObject>("ObjectIdentifier", entity =>
        {
            // Handle updates as the come
            Console.WriteLine($"Update received. Value is: {entity.SomeText}");
        });
    }
}
﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace RemoteEntity.Redis;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddRemoteEntityWithRedisBackEnd(this IHostApplicationBuilder hostBuilder)
    {
        hostBuilder.AddRemoteEntityCore();
        hostBuilder.Services
            .Configure<RedisConnectionOptions>(hostBuilder.Configuration.GetSection("RemoteEntity"))
            .AddSingleton(sp => sp.GetRequiredService<IOptions<RedisConnectionOptions>>().Value)
            .AddSingleton<IRedisConnection, RedisConnection>()
            .AddSingleton<IEntityStorage, RedisEntityStorage>()
            .AddSingleton<IEntityPubSub, RedisEntityPubSub>()
            .AddTransient<IDuplicateDetector, DuplicateDetector>()
            .AddSingleton<HiveOptions>();
        return hostBuilder;
    }
}
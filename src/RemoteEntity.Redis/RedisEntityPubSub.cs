using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace RemoteEntity.Redis
{
    public class RedisEntityPubSub : IEntityPubSub
    {
        protected IConnectionMultiplexer redisDb => redisConnection.Multiplexer;
        private readonly IRedisConnection redisConnection;
        private readonly ILogger logger;
        private readonly string streamPrefix;

        private HashSet<string> subscribers { get; } = new();

        public RedisEntityPubSub(IRedisConnection redisConnection, ILogger<RedisEntityPubSub> logger)
        {
            this.redisConnection = redisConnection;
            this.logger = logger;
            streamPrefix = "entitystream.";
        }

        public RedisEntityPubSub(IRedisConnection redisConnection, string streamPrefix)
        {
            this.redisConnection = redisConnection;
            this.streamPrefix = streamPrefix;

        }

        private string getStreamName(string entityId)
        {
            return $"{streamPrefix}{entityId}".ToLower();
        }

        public void Publish<T>(string entityId, EntityDto<T> entity)
        {
            var serialized = JsonSerializer.Serialize(entity);
            var streamName = getStreamName(entityId);
            redisDb.GetSubscriber().Publish(
                new RedisChannel(streamName, RedisChannel.PatternMode.Auto),
                Encoding.UTF8.GetBytes(serialized)
            );
        }
        
        public void Subscribe<T>(string entityId, Action<EntityDto<T>> handler)
        {
            var streamName = getStreamName(entityId);
            if (subscribers.Contains(streamName))
            {
                return;
            }

            subscribers.Add(streamName);
            
            redisDb.GetSubscriber().Subscribe(
                new RedisChannel(streamName, RedisChannel.PatternMode.Auto),
                handler: (channel, value) =>
                {
                    var bytes = (byte[])value;
                    if (bytes == null) return;

                    try
                    {
                        var serializedContent = Encoding.UTF8.GetString(bytes);
                        var deserializedEntity = JsonSerializer.Deserialize<EntityDto<T>>(serializedContent);

                        if (handler != null && deserializedEntity != null)
                        {
                            handler(deserializedEntity);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error while handling incoming message");
                    }
                }
            );
        }

        public void Unsubscribe(string entityId)
        {
            redisDb.GetSubscriber().Unsubscribe(
                new RedisChannel(getStreamName(entityId), RedisChannel.PatternMode.Auto)
            );
        }
    }
}

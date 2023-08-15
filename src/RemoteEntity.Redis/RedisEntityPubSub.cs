using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace RemoteEntity.Redis
{
    public class RedisEntityPubSub : IEntityPubSub
    {
        private readonly ConnectionMultiplexer redisDb;
        private readonly string streamPrefix;

        private HashSet<string> subscribers { get; } = new();

        public RedisEntityPubSub(ConnectionMultiplexer redisDb)
        {
            this.redisDb = redisDb;
            streamPrefix = "entitystream.";
        }

        public RedisEntityPubSub(ConnectionMultiplexer redisDb, string streamPrefix)
        {
            this.redisDb = redisDb;
            this.streamPrefix = streamPrefix;

        }

        private string getStreamName(string entityId)
        {
            return $"{streamPrefix}{entityId}".ToLower();
        }

        public void Publish<T>(string entityId, EntityDto<T> entity)
        {
            var serialized = JsonConvert.SerializeObject(entity);
            redisDb.GetSubscriber().Publish(
                new RedisChannel(getStreamName(entityId), RedisChannel.PatternMode.Auto),
                Encoding.UTF8.GetBytes(serialized)
            );
        }
        
        public void Subscribe<T>(string entityId, Action<EntityDto<T>> handler)
        {
            var streamName = getStreamName(entityId);
            if (subscribers.Contains(streamName))
                return;

            subscribers.Add(streamName);
            
            redisDb.GetSubscriber().Subscribe(
                new RedisChannel(streamName, RedisChannel.PatternMode.Auto),
                handler: (channel, value) =>
                {
                    var bytes = (byte[])value;
                    if (bytes == null) return;
                    
                    var serializedContent = Encoding.UTF8.GetString(bytes);
                    var deserializedEntity = JsonConvert.DeserializeObject<EntityDto<T>>(serializedContent); //todo if this fails should an event be emitted from the framework?

                    if (handler != null && deserializedEntity != null)
                    {
                        handler(deserializedEntity);
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

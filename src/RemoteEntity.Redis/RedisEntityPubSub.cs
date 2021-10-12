using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServiceStack.Redis;

namespace RemoteEntity.Redis
{
    public class RedisEntityPubSub : IEntityPubSub
    {
        private readonly RedisClient redisClient;
        private readonly ILogger logger;

        public RedisEntityPubSub(RedisClient redisClient, ILogger logger)
        {
            this.redisClient = redisClient;
            this.logger = logger;
        }

        public void Publish<T>(string channel, EntityDto<T> dto)
        {
            var serialized = JsonConvert.SerializeObject(dto);
            redisClient.Publish(channel, Encoding.UTF8.GetBytes(serialized));
        }

        public Task Subscribe<T>(string channel, Action<EntityDto<T>> handler)
        {
            return Task.Run(async () =>
            {
                var subscription = redisClient.CreateSubscription();

                subscription.OnMessageBytes = (s, bytes) =>
                {
                    EntityDto<T> deserializedEntity = null;
                    try
                    {
                        var serializedContent = Encoding.UTF8.GetString(bytes);
                        deserializedEntity = JsonConvert.DeserializeObject<EntityDto<T>>(serializedContent);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"Error while deserializing EntityDto of type '{typeof(T).ToString()}' on channel '{channel}'");
                    }

                    if (handler != null && deserializedEntity != null)
                    {
                        handler(deserializedEntity);
                    }

                };
                subscription.SubscribeToChannels(channel);

            });
        }

        public void Unsubscribe(string channel)
        {
            Task.Run(async () =>
            {
                redisClient.UnSubscribe(channel);
            });

        }

    }
}

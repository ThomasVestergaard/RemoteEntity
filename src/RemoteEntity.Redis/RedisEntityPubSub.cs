using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServiceStack.Redis;

namespace RemoteEntity.Redis
{
    public class RedisEntityPubSub : IEntityPubSub
    {
        private IRedisClient redisClient
        {
            get
            {
                return redisClientManager.GetClient();
            }
        }

        private readonly IRedisClientsManager redisClientManager;
        private readonly ILogger logger;
        private string streamPrefix { get; set; }

        public RedisEntityPubSub(IRedisClientsManager redisClientManager, ILogger logger)
        {
            this.redisClientManager = redisClientManager;
            this.logger = logger;
            streamPrefix = "entitystream.";
        }

        public RedisEntityPubSub(IRedisClientsManager redisClientManager, ILogger logger, string streamPrefix)
        {
            this.redisClientManager = redisClientManager;
            this.logger = logger;
            this.streamPrefix = streamPrefix;
        }

        private string getStreamName(string entityId)
        {
            return $"{streamPrefix}{entityId}".ToLower();
        }

        public void Publish<T>(string entityId, EntityDto<T> dto)
        {
            var serialized = JsonConvert.SerializeObject(dto);
            ((RedisClient)redisClient).Publish(getStreamName(entityId), Encoding.UTF8.GetBytes(serialized));
        }

        public Task Subscribe<T>(string entityId, Action<EntityDto<T>> handler)
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
                        logger.LogError(ex, $"Error while deserializing EntityDto of type '{typeof(T).ToString()}' on channel '{getStreamName(entityId)}'");
                    }

                    if (handler != null && deserializedEntity != null)
                    {
                        handler(deserializedEntity);
                    }

                };
                subscription.SubscribeToChannels(getStreamName(entityId));

            });
        }

        public void Unsubscribe(string entityId)
        {
            Task.Run(async () =>
            {
                ((RedisClient)redisClient).UnSubscribe(getStreamName(entityId));
            });

        }

    }
}

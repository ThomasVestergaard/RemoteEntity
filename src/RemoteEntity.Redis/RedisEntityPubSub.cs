using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BeetleX.Redis;
using Newtonsoft.Json;

namespace RemoteEntity.Redis
{
    public class RedisEntityPubSub : IEntityPubSub
    {
        private readonly RedisDB redisDb;
        private readonly string streamPrefix;

        private Dictionary<string, Subscriber> subscribers { get; set; }

        public RedisEntityPubSub(RedisDB redisDb)
        {
            this.redisDb = redisDb;
            streamPrefix = "entitystream.";
            subscribers = new Dictionary<string, Subscriber>();
        }

        public RedisEntityPubSub(RedisDB redisDb, string streamPrefix)
        {
            this.redisDb = redisDb;
            this.streamPrefix = streamPrefix;
            subscribers = new Dictionary<string, Subscriber>();
        }

        private string getStreamName(string entityId)
        {
            return $"{streamPrefix}{entityId}".ToLower();
        }

        public void Publish<T>(string entityId, EntityDto<T> entity)
        {
            var serialized = JsonConvert.SerializeObject(entity);
            redisDb.Publish(getStreamName(entityId), Encoding.UTF8.GetBytes(serialized)).GetAwaiter().GetResult();
        }
        
        public void Subscribe<T>(string entityId, Action<EntityDto<T>> handler)
        {
            if (subscribers.ContainsKey(getStreamName(entityId)))
                return;

            var subscriber = redisDb.Subscribe();
            subscriber.Register<byte[]>(getStreamName(entityId), dtoBytes =>
            {
                var serializedContent = Encoding.UTF8.GetString(dtoBytes);
                var deserializedEntity = JsonConvert.DeserializeObject<EntityDto<T>>(serializedContent);

                if (handler != null && deserializedEntity != null)
                {
                    handler(deserializedEntity);
                }
            });

            subscriber.Listen();
            subscribers.Add(getStreamName(entityId), subscriber);
        }

        public void Unsubscribe(string entityId)
        {
            if (subscribers.ContainsKey(getStreamName(entityId)))
            {
                subscribers[getStreamName(entityId)].UnRegister(getStreamName(entityId));
                subscribers.Remove(getStreamName(entityId));
            }
            
        }
    }
}

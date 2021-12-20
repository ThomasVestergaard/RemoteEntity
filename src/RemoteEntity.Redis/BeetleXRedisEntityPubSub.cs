using System;
using System.Text;
using System.Threading.Tasks;
using BeetleX.Redis;
using Newtonsoft.Json;

namespace RemoteEntity.Redis
{
    public class BeetleXRedisEntityPubSub : IEntityPubSub
    {
        private readonly RedisDB redisDb;
        private readonly string streamPrefix;

        public BeetleXRedisEntityPubSub(RedisDB redisDb, string streamPrefix)
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
            redisDb.Publish(getStreamName(entityId), Encoding.UTF8.GetBytes(serialized)).GetAwaiter().GetResult();
        }
        
        public Task Subscribe<T>(string entityId, Action<EntityDto<T>> handler)
        {
            /*
            var subber = DefaultRedis.Subscribe();
            var subscription = subber.Register<string>("testchannel", dto =>
            {
                Console.WriteLine($"Received: {dto}");

            });
            subscription.Listen();
            */

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

            return Task.CompletedTask;
        }

        public void Unsubscribe(string entityId)
        {
            throw new NotImplementedException();
        }
    }
}

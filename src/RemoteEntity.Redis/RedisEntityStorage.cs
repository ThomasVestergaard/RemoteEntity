using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using ServiceStack.Redis;

namespace RemoteEntity.Redis
{
    public class RedisEntityStorage : IEntityStorage
    {
        private readonly RedisClient redisClient;
        private readonly ILogger logger;

        public RedisEntityStorage(RedisClient redisClient, ILogger logger)
        {
            this.redisClient = redisClient;
            this.logger = logger;
        }
        public bool ContainsKey(string key)
        {
            return redisClient.ContainsKey(key);
        }

        public bool Add<T>(string key, T entity)
        {
            var serialized = JsonConvert.SerializeObject(entity);
            return redisClient.Add(key, serialized);
        }

        public bool Set<T>(string key, T entity)
        {
            var serialized = JsonConvert.SerializeObject(entity);
            return redisClient.Set(key, serialized);
        }

        public T Get<T>(string key)
        {
            var serialized = redisClient.Get<string>(key);
            return JsonConvert.DeserializeObject<T>(serialized);
        }


    }
}

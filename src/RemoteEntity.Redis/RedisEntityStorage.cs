using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using ServiceStack.Redis;

namespace RemoteEntity.Redis
{
    public class RedisEntityStorage : IEntityStorage
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
        private string keyPrefix { get; set; }

        public RedisEntityStorage(IRedisClientsManager redisClientManager, ILogger logger)
        {
            this.redisClientManager = redisClientManager;
            this.logger = logger;
            keyPrefix = "entitystate.";
        }

        public RedisEntityStorage(IRedisClientsManager redisClientManager, ILogger logger, string keyPrefix)
        {
            this.redisClientManager = redisClientManager;
            this.logger = logger;
            this.keyPrefix = keyPrefix;
        }

        private string getKeyName(string entityId)
        {
            return $"{keyPrefix}{entityId}".ToLower();
        }

        public bool ContainsKey(string key)
        {
            return redisClient.ContainsKey(getKeyName(key));
        }

        public bool Add<T>(string key, T entity)
        {
            var serialized = JsonConvert.SerializeObject(entity);
            return redisClient.Add(getKeyName(key), serialized);
        }

        public bool Set<T>(string key, T entity)
        {
            var serialized = JsonConvert.SerializeObject(entity);
            return redisClient.Set(getKeyName(key), serialized);
        }

        public T Get<T>(string key)
        {
            var serialized = redisClient.Get<string>(getKeyName(key));
            return JsonConvert.DeserializeObject<T>(serialized);
        }


    }
}

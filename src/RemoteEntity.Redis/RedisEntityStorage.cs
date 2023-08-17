using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace RemoteEntity.Redis
{
    public class RedisEntityStorage : IEntityStorage
    {
        private readonly ConnectionMultiplexer redisDb;
        private readonly ILogger logger;
        private readonly string keyPrefix;

        public RedisEntityStorage(ConnectionMultiplexer redisDb, ILogger<RedisEntityStorage> logger)
        {
            this.redisDb = redisDb;
            this.logger = logger;
            keyPrefix = "entitystate.";
        }
        public RedisEntityStorage(ConnectionMultiplexer redisDb, ILogger<RedisEntityStorage> logger, string keyPrefix)
        {
            this.redisDb = redisDb;
            this.keyPrefix = keyPrefix;
            this.logger = logger;
        }

        private string getKeyName(string entityId)
        {
            return $"{keyPrefix}{entityId}".ToLower();
        }

        public bool ContainsKey(string key)
        {
            if (redisDb.GetDatabase().KeyExists(getKeyName(key)))
                return true;

            return false;
        }

        public bool Add<T>(string key, T entity) => Set(key, entity);

        public bool Set<T>(string key, T entity)
        {
            try
            {
                var serialized = JsonConvert.SerializeObject(entity);
                redisDb.GetDatabase().StringSet(getKeyName(key), serialized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to set entity");
                return false;
            }

            return true;
        }

        public T Get<T>(string key)
        {
            try
            {
                var serialized = redisDb.GetDatabase().StringGet(getKeyName(key));
                return JsonConvert.DeserializeObject<T>(serialized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get entity");
                return default;
            }
        }

        public void Delete(string key) => redisDb.GetDatabase().KeyDelete(getKeyName(key));
    }
}

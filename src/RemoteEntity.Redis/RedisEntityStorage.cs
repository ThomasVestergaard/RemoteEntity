using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace RemoteEntity.Redis
{
    public class RedisEntityStorage : IEntityStorage
    {
        protected IConnectionMultiplexer redisDb => redisConnection.Multiplexer;
        private readonly IRedisConnection redisConnection;
        private readonly ILogger logger;
        private readonly string keyPrefix;

        public RedisEntityStorage(IRedisConnection redisConnection, ILogger<RedisEntityStorage> logger)
        {
            this.redisConnection = redisConnection;
            this.logger = logger;
            keyPrefix = "entitystate.";
        }
        public RedisEntityStorage(IRedisConnection redisConnection, ILogger<RedisEntityStorage> logger, string keyPrefix)
        {
            this.redisConnection = redisConnection;
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
                var serialized = JsonSerializer.Serialize(entity);
                redisDb.GetDatabase().StringSet(getKeyName(key), serialized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to set entity");
                return false;
            }

            return true;
        }

        public bool SetEntityDto<T>(string key, IEntityDto<T> entityDto)
        {
            try
            {
                var serialized = JsonSerializer.Serialize(entityDto);
                redisDb.GetDatabase().StringSet(getKeyName(key), serialized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to set entity dto");
                return false;
            }

            return true;
        }

        public IEntityDto<T> GetEntityDto<T>(string key)
        {
            try
            {
                var serialized = redisDb.GetDatabase().StringGet(getKeyName(key));
                return JsonSerializer.Deserialize<EntityDto<T>>(serialized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get entity");
                return default;
            }
        }

        public string GetRaw(string key)
        {
            try
            {
                return redisDb.GetDatabase().StringGet(getKeyName(key));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get raw entity");
                return String.Empty;
            }
        }
        
        public T Get<T>(string key)
        {
            try
            {
                var serialized = redisDb.GetDatabase().StringGet(getKeyName(key));
                return JsonSerializer.Deserialize<T>(serialized);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get entity");
                return default(T);
            }
        }

        public List<string> GetKeys(int maxKeys = 1000)
        {
            try
            {
                var keys = redisDb.GetServers().First().Keys(redisDb.GetDatabase().Database, $"{keyPrefix}*", maxKeys);
                return keys.Select(k => k.ToString().Replace(keyPrefix, "")).ToList();
                
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get all keys");
                return default(List<string>);
            }
        }

        public void Delete(string key) => redisDb.GetDatabase().KeyDelete(getKeyName(key));
        public void Start()
        {
            redisConnection.Start();
        }
    }
}

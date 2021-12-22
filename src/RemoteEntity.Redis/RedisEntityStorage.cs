using BeetleX.Redis;
using Newtonsoft.Json;

namespace RemoteEntity.Redis
{
    public class RedisEntityStorage : IEntityStorage
    {
        private readonly RedisDB redisDb;
        private readonly string keyPrefix;

        public RedisEntityStorage(RedisDB redisDb)
        {
            this.redisDb = redisDb;
            keyPrefix = "entitystate.";
        }
        public RedisEntityStorage(RedisDB redisDb, string keyPrefix)
        {
            this.redisDb = redisDb;
            this.keyPrefix = keyPrefix;
        }

        private string getKeyName(string entityId)
        {
            return $"{keyPrefix}{entityId}".ToLower();
        }

        public bool ContainsKey(string key)
        {
            if (redisDb.Exists(getKeyName(key)).GetAwaiter().GetResult() == 1)
                return true;

            return false;
        }

        public bool Add<T>(string key, T entity)
        {
            var serialized = JsonConvert.SerializeObject(entity);
            redisDb.Set(getKeyName(key), serialized).GetAwaiter().GetResult();
            return true;
        }

        public bool Set<T>(string key, T entity)
        {
            var serialized = JsonConvert.SerializeObject(entity);
            redisDb.Set(getKeyName(key), serialized).GetAwaiter().GetResult();
            return true;
        }

        public T Get<T>(string key)
        {
            var serialized = redisDb.Get<string>(getKeyName(key)).GetAwaiter().GetResult();
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }
}

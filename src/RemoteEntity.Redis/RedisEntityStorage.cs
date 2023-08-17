using Newtonsoft.Json;
using StackExchange.Redis;

namespace RemoteEntity.Redis
{
    public class RedisEntityStorage : IEntityStorage
    {
        private readonly ConnectionMultiplexer redisDb;
        private readonly string keyPrefix;

        public RedisEntityStorage(ConnectionMultiplexer redisDb)
        {
            this.redisDb = redisDb;
            keyPrefix = "entitystate.";
        }
        public RedisEntityStorage(ConnectionMultiplexer redisDb, string keyPrefix)
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
            if (redisDb.GetDatabase().KeyExists(getKeyName(key)))
                return true;

            return false;
        }

        public bool Add<T>(string key, T entity) => Set(key, entity);

        public bool Set<T>(string key, T entity)
        {
            var serialized = JsonConvert.SerializeObject(entity);
            redisDb.GetDatabase().StringSet(getKeyName(key), serialized);
            return true;
        }

        public T Get<T>(string key)
        {
            var serialized = redisDb.GetDatabase().StringGet(getKeyName(key));
            return JsonConvert.DeserializeObject<T>(serialized);
        }

        public void Delete(string key) => redisDb.GetDatabase().KeyDelete(getKeyName(key));
    }
}

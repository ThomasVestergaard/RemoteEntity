
using System.Collections.Generic;

namespace RemoteEntity
{
    public interface IEntityStorage
    {
        bool ContainsKey(string key);
        bool Add<T>(string key, T entity);
        bool Set<T>(string key, T entity);
        T Get<T>(string key);
        string GetRaw(string key);
        List<string> GetKeys(int maxKeys = 1000);
        void Delete(string key);
    }


}

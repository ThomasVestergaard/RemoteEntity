
using System.Collections.Generic;

namespace RemoteEntity
{
    public interface IEntityStorage
    {
        bool ContainsKey(string key);
        bool Add<T>(string key, T entity);
        bool Set<T>(string key, T entity);
        T Get<T>(string key);
        IEnumerable<string> GetKeys();
    }


}

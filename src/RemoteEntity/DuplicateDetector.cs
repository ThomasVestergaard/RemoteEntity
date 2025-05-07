using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace RemoteEntity;

public class DuplicateDetector
{
    private readonly ConcurrentDictionary<string, string> hashMap = new();
    private readonly MD5 md5 = MD5.Create();

    private string HashEntity<T>(T entity)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(entity);
        return Convert.ToHexString(md5.ComputeHash(Encoding.UTF8.GetBytes(json))).ToLowerInvariant();
    }

    public bool IsDuplicate<T>(T entity, string entityId)
    {
        var hashedEntity = HashEntity(entity);

        if (hashMap.TryAdd(entityId, hashedEntity))
        {
            return false;
        }

        if (hashMap[entityId] == hashedEntity)
        {
            return true;
        }

        hashMap[entityId] = hashedEntity;
        return false;
    }
}
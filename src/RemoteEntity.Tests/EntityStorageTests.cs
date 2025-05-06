using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using RemoteEntity.Redis;
using StackExchange.Redis;

namespace RemoteEntity.Tests;

[TestFixture]
public class EntityStorageTests
{
    private RedisEntityStorage RedisEntityStorage;
    private ConcurrentBag<string> _createdEntityIds = new();
    private RedisConnectionOptions redisConfig;
    private RedisConnection redisConnection;

    [SetUp]
    public void Setup()
    {
        redisConfig= new RedisConnectionOptions
        {
            RedisHostName = "localhost",
        };
        redisConnection = new RedisConnection(redisConfig);
        redisConnection.Start();
        RedisEntityStorage  = new(redisConnection, NullLogger<RedisEntityStorage>.Instance);
    }
    
    
    [TearDown]
    public void CleanUp()
    {
        foreach (var createdEntityId in _createdEntityIds)
            Safe.Try(() => RedisEntityStorage.Delete(createdEntityId));
    }
    
    [Test]
    public void EntityCanBeAddedSuccessfullyTest()
    {
        var key = CreateEntityKey();
        var entity = new Entity { Value = "SomeValue" };
        
        RedisEntityStorage.Add(key, entity);
        var fetchedEntity = RedisEntityStorage.Get<Entity>(key);
        
        Assert.That("SomeValue", Is.EqualTo(fetchedEntity.Value));
    }
    
    [Test]
    public void ContainsKeyOfExistingKeyReturnsTrue()
    {
        var key = CreateEntityKey();
        
        var entity = new Entity { Value = "SomeValue" };
        RedisEntityStorage.Add(key, entity);
            
        RedisEntityStorage.Delete(key);
        var containsKey = RedisEntityStorage.ContainsKey(key);
        Assert.That(containsKey, Is.False);
    }
    
    [Test]
    public void ContainsKeyOfDeletedKeyReturnsFalse()
    {
        var key = CreateEntityKey();
        var entity = new Entity { Value = "SomeValue" };
        RedisEntityStorage.Add(key, entity);
            
        RedisEntityStorage.Delete(key);
        var containsKey = RedisEntityStorage.ContainsKey(key);
        Assert.That(containsKey, Is.False);
    }
    
    [Test]
    public void ContainsKeyOfNonExistingKeyReturnsFalse()
    {
        var key = CreateEntityKey();
        
        var containsKey = RedisEntityStorage.ContainsKey(key);
        Assert.That(containsKey, Is.False);
    }

    private class Entity
    {
        public string Value { get; set; }
    }

    private string CreateEntityKey()
    {
        var id = Guid.NewGuid().ToString();
        _createdEntityIds.Add(id);
        return id;
    }
}
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
    private static readonly RedisEntityStorage RedisEntityStorage = new(ConnectionMultiplexer.Connect("localhost"), NullLogger<RedisEntityStorage>.Instance);
    
    private readonly ConcurrentBag<string> _createdEntityIds = new();

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
        
        Assert.AreEqual("SomeValue", fetchedEntity.Value);
    }
    
    [Test]
    public void ContainsKeyOfExistingKeyReturnsTrue()
    {
        var key = CreateEntityKey();
        
        var entity = new Entity { Value = "SomeValue" };
        RedisEntityStorage.Add(key, entity);
            
        RedisEntityStorage.Delete(key);
        var containsKey = RedisEntityStorage.ContainsKey(key);
        Assert.IsFalse(containsKey);
    }
    
    [Test]
    public void ContainsKeyOfDeletedKeyReturnsFalse()
    {
        var key = CreateEntityKey();
        var entity = new Entity { Value = "SomeValue" };
        RedisEntityStorage.Add(key, entity);
            
        RedisEntityStorage.Delete(key);
        var containsKey = RedisEntityStorage.ContainsKey(key);
        Assert.IsFalse(containsKey);
    }
    
    [Test]
    public void ContainsKeyOfNonExistingKeyReturnsFalse()
    {
        var key = CreateEntityKey();
        
        var containsKey = RedisEntityStorage.ContainsKey(key);
        Assert.IsFalse(containsKey);
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
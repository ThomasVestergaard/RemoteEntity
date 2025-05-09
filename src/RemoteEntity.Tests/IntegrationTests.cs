using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using RemoteEntity.Redis;

namespace RemoteEntity.Tests;

[TestFixture]
public class IntegrationTests
{
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
    }
    
    [Test]
    public async Task PublishedEntityIsReceivedBySubscriberSuccessfully()
    {
        var redisEntityStorage = new RedisEntityStorage(redisConnection, NullLogger<RedisEntityStorage>.Instance);
        var redisEntityPubSub = new RedisEntityPubSub(redisConnection, NullLogger<RedisEntityPubSub>.Instance);
        var entityHive = new EntityHive(redisEntityStorage, redisEntityPubSub, new HiveOptions(), new DuplicateDetector(), NullLogger<EntityHive>.Instance);
        var entityId = Guid.NewGuid().ToString();
        
        try
        {
            var flag = new Flag();
            Entity entity = null;
            
            var observer = entityHive
                .SubscribeToEntity<Entity>(
                    entityId,
                    updateHandler: e =>
                    {
                        entity = e;
                        flag.Raise();
                    }
                );

            await Task.Delay(500);
            Assert.That(FlagPosition.Lowered, Is.EqualTo(flag.Position));
            Assert.That(observer.Value, Is.Null);

            
            entityHive.PublishEntity(new Entity { Value = "SomeValue" }, entityId);
            await flag.WaitForRaised(maxWait: TimeSpan.FromSeconds(5));
            

            observer.Stop();

            Assert.That("SomeValue", Is.EqualTo(entity.Value));
        }
        finally
        {
            redisEntityStorage.Delete(entityId);
        }
    } 
  
    private class Entity : ICloneable<Entity>
    { 
        public string Value { get; set; }
    }
}
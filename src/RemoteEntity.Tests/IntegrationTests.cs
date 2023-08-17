using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using RemoteEntity.Redis;
using StackExchange.Redis;

namespace RemoteEntity.Tests;

[TestFixture]
public class IntegrationTests
{
    private static readonly ConnectionMultiplexer RedisConnection = ConnectionMultiplexer.Connect("localhost");
    
    [Test]
    public async Task PublishedEntityIsReceivedBySubscriberSuccessfully()
    {
        var redisEntityStorage = new RedisEntityStorage(RedisConnection, NullLogger<RedisEntityStorage>.Instance);
        var redisEntityPubSub = new RedisEntityPubSub(RedisConnection, NullLogger<RedisEntityPubSub>.Instance);
        var entityHive = new EntityHive(redisEntityStorage, redisEntityPubSub, NullLogger<EntityHive>.Instance);
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

            await Task.Delay(100);
            Assert.AreEqual(FlagPosition.Lowered, flag.Position);
            Assert.IsNull(observer.Value);

            entityHive.PublishEntity(new Entity { Value = "SomeValue" }, entityId);

            await flag.WaitForRaised(maxWait: TimeSpan.FromSeconds(5));

            observer.Stop();

            Assert.AreEqual("SomeValue", entity.Value);
        }
        finally
        {
            redisEntityStorage.Delete(entityId);
        }
    } 

    
    private class Entity : ICloneable<Entity>
    {
        public string Value { get; set; }

        public Entity Clone() => new() { Value = Value };
    }
}
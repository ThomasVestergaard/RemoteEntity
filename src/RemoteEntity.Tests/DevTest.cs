using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using RemoteEntity.Redis;
using RemoteEntity.Tags;

namespace RemoteEntity.Tests;

[TestFixture]
public class DevTest
{
    [Test]
    public void TagsStoreGetTest()
    {
        var redisConnection = new RedisConnection(new RedisConnectionOptions
        {
            RedisHostName = "localhost"
        });
        var redisStorage = new RedisEntityStorage(redisConnection, new NullLogger<RedisEntityStorage>());
        redisConnection.Start();

        var tags = new List<EntityTag>();
        tags.Add(EntityTag.Create("tag1", "string value"));
        tags.Add(EntityTag.Create("tag2", false));
        var entity = new SomeValueObject
        {
            SomeNumber = 456,
            SomeText = "Some text"
        };
        var id = "dto_id";
        var dto = new EntityDto<SomeValueObject>(id, entity, DateTimeOffset.Now, tags);
        redisStorage.SetEntityDto(id, dto);

        var raw = redisStorage.GetRaw(id);
        var fetchedEntity = redisStorage.GetEntityDto<SomeValueObject>(id);

        int halt = 0;

    }
    
    private class SomeValueObject : ICloneable<SomeValueObject>, IInitialSeed<SomeValueObject>
    {
        // This is the object that is sent fra the producer to the consumer. This could be put in a shared class library.

        public string SomeText { get; set; } = null!;
        public decimal SomeNumber { get; set; }

        public SomeValueObject InitialSeedEntity()
        {
            return new SomeValueObject()
            {
                SomeNumber = -1,
                SomeText = "Default text value"
            };
        }

        public IEnumerable<IEntityTag> InitialSeedEntityTags()
        {
            return new List<IEntityTag>();
        }
    }
    
    
}
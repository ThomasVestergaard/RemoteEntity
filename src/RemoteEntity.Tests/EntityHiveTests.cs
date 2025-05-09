using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace RemoteEntity.Tests;

[TestFixture]
public class EntityHiveTests
{
    private Mock<IEntityStorage> entityStorageMock;
    private Mock<IEntityPubSub> entityPubSubMock;
    private Mock<IDuplicateDetector> duplicateDetectorMock;
    private HiveOptions hiveOptions;
    private Mock<ILogger<EntityHive>> loggerMock;
    
    [SetUp]
    public void Setup()
    {
        entityStorageMock = new Mock<IEntityStorage>();
        entityPubSubMock = new Mock<IEntityPubSub>();
        duplicateDetectorMock = new Mock<IDuplicateDetector>();
        loggerMock = new Mock<ILogger<EntityHive>>();
    }

    [Test]
    public async Task ShouldAllowPublishDuplicatedAsDefault()
    {
        var options = new HiveOptions();
        Assert.That(options.PublishDuplicates, Is.True);
    }
    
    [Test]
    public async Task ShouldPublishDuplicate_WhenDuplicateDetected()
    {
        hiveOptions = new HiveOptions
        {
            PublishDuplicates = true
        };

        duplicateDetectorMock
            .SetupSequence(a => a.IsDuplicate(It.IsAny<Entity>(), It.IsAny<string>()))
            .Returns(false)
            .Returns(true);
        
        var hive = new EntityHive(entityStorageMock.Object, entityPubSubMock.Object, hiveOptions, duplicateDetectorMock.Object, loggerMock.Object);
        var entity1 = new Entity { Value = "1" };
        var entity2 = new Entity { Value = "1" };

        hive.PublishEntity(entity1, "1");
        hive.PublishEntity(entity2, "1");
        
        entityPubSubMock.Verify(a => a.Publish<Entity>(
            It.Is<string>(s => s == "1"), 
            It.IsAny<EntityDto<Entity>>()), Times.Exactly(2)
            );
    }
    
    [Test]
    public async Task ShouldSkipDuplicate_WhenDuplicateDetected()
    {
        hiveOptions = new HiveOptions
        {
            PublishDuplicates = false
        };

        duplicateDetectorMock
            .SetupSequence(a => a.IsDuplicate(It.IsAny<Entity>(), It.IsAny<string>()))
            .Returns(false)
            .Returns(true);
        
        var hive = new EntityHive(entityStorageMock.Object, entityPubSubMock.Object, hiveOptions, duplicateDetectorMock.Object, loggerMock.Object);
        var entity1 = new Entity { Value = "1" };
        var entity2 = new Entity { Value = "1" };

        hive.PublishEntity(entity1, "1");
        hive.PublishEntity(entity2, "1");
        
        entityPubSubMock.Verify(a => a.Publish<Entity>(
                It.Is<string>(s => s == "1"), 
                It.IsAny<EntityDto<Entity>>()), Times.Exactly(1)
        );
    }
    
    private class Entity : ICloneable<Entity>
    { 
        public string Value { get; set; }
    }
}


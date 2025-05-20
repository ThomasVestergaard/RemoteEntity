using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using RemoteEntity.Stats;

namespace RemoteEntity.Tests;

[TestFixture]
public class EntityHiveTests
{
    private Mock<IEntityStorage> entityStorageMock;
    private Mock<IEntityPubSub> entityPubSubMock;
    private Mock<IDuplicateDetector> duplicateDetectorMock;
    private HiveOptions hiveOptions;
    private Mock<ILogger<EntityHive>> loggerMock;
    private Mock<IStatsSinkManager> statsSinksManagerMock;
    
    [SetUp]
    public void Setup()
    {
        entityStorageMock = new Mock<IEntityStorage>();
        entityPubSubMock = new Mock<IEntityPubSub>();
        duplicateDetectorMock = new Mock<IDuplicateDetector>();
        loggerMock = new Mock<ILogger<EntityHive>>();
        statsSinksManagerMock = new Mock<IStatsSinkManager>();
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
        
        var hive = new EntityHive(entityStorageMock.Object, entityPubSubMock.Object, hiveOptions, duplicateDetectorMock.Object, loggerMock.Object, statsSinksManagerMock.Object);
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
        
        var hive = new EntityHive(entityStorageMock.Object, entityPubSubMock.Object, hiveOptions, duplicateDetectorMock.Object, loggerMock.Object, statsSinksManagerMock.Object);
        var entity1 = new Entity { Value = "1" };
        var entity2 = new Entity { Value = "1" };

        hive.PublishEntity(entity1, "1");
        hive.PublishEntity(entity2, "1");
        
        entityPubSubMock.Verify(a => a.Publish<Entity>(
                It.Is<string>(s => s == "1"), 
                It.IsAny<EntityDto<Entity>>()), Times.Exactly(1)
        );
    }

    [Test]
    public void ShouldCallStatsManagerOnPublish()
    {
        hiveOptions = new HiveOptions();
        entityPubSubMock.Setup(a => a.Publish(It.IsAny<string>(), It.IsAny<EntityDto<Entity>>())).Returns(500);
        duplicateDetectorMock.Setup(a => a.IsDuplicate(It.IsAny<Entity>(), It.IsAny<string>())).Returns(true);
        var hive = new EntityHive(entityStorageMock.Object, entityPubSubMock.Object, hiveOptions, duplicateDetectorMock.Object, loggerMock.Object, statsSinksManagerMock.Object);
        var entity1 = new Entity { Value = "1" };
        
        hive.PublishEntity(entity1, "1");

        statsSinksManagerMock.Verify(a => a.RegisterPublish(
            It.Is<string>(s => s == "1"),
            It.Is<string>(s => s == typeof(Entity).FullName!),
            It.Is<long>(s => s == 500)), Times.Once);
    }

    [Test]
    public async Task ShouldCallEntityStorageStart_OnStart()
    {
        var hive = new EntityHive(entityStorageMock.Object, entityPubSubMock.Object, hiveOptions, duplicateDetectorMock.Object, loggerMock.Object, statsSinksManagerMock.Object);
        await hive.Start();
        
        entityStorageMock.Verify(s => s.Start(), Times.Once);
    }
    
    [Test]
    public async Task ShouldCallEntityPubSubStart_OnStart()
    {
        var hive = new EntityHive(entityStorageMock.Object, entityPubSubMock.Object, hiveOptions, duplicateDetectorMock.Object, loggerMock.Object, statsSinksManagerMock.Object);
        await hive.Start();
        
        entityPubSubMock.Verify(s => s.Start(), Times.Once);
    }
    
    [Test]
    public async Task ShouldCallSinkManagerStart_OnStart()
    {
        var hive = new EntityHive(entityStorageMock.Object, entityPubSubMock.Object, hiveOptions, duplicateDetectorMock.Object, loggerMock.Object, statsSinksManagerMock.Object);
        await hive.Start();
        
        statsSinksManagerMock.Verify(s => s.Start(), Times.Once);
    }
    
    private class Entity : ICloneable<Entity>
    { 
        public string Value { get; set; }
    }
}


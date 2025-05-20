using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using RemoteEntity.Stats;

namespace RemoteEntity.Tests;

[TestFixture]
public class StatsSinkManagerTests
{
    private Mock<IRemoteEntityStatsSink> sink1;
    private Mock<IRemoteEntityStatsSink> sink2;
    
    
    [SetUp]
    public void Setup()
    {
        sink1 = new Mock<IRemoteEntityStatsSink>();
        sink2 = new Mock<IRemoteEntityStatsSink>();
    }

    [Test]
    public async Task ShouldCallStartOnSinks_OnStart()
    {
        var manager = new StatsSinkManager(new []{ sink1.Object, sink2.Object });
        manager.Start();
        
        sink1.Verify(s => s.Start(), Times.Once);
        sink2.Verify(s => s.Start(), Times.Once);
    }

    [Test]
    public async Task ShouldDispatch3StatsToEachSink_OnRegisterPublish()
    {
        var manager = new StatsSinkManager(new []{ sink1.Object, sink2.Object });
        manager.Start();
        manager.RegisterPublish("some-id", "some-entity-type", 1000);
        
        sink1.Verify(s => s.ProcessStats(It.Is<IEnumerable<StatEntry>>(stats => stats.Count() == 3)), Times.Once);
        sink2.Verify(s => s.ProcessStats(It.Is<IEnumerable<StatEntry>>(stats => stats.Count() == 3)), Times.Once);
    }
    

}
using Microsoft.Extensions.Hosting;
using RemoteEntity;

namespace Example.StatsSink.Console;

public class Producer(IEntityHive entityHive) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        System.Console.WriteLine("Starting producer");
        var obj = new SomeValueObject
        {
            SomeNumber = 456.55m,
            SomeText = "Update"
        };

        for(int i=0; i<5; i++)
        {
            // Publish 10 messages
            obj.SomeText = $"Update no {i}";
            
            try
            {
                entityHive.PublishEntity(obj, "ObjectIdentifier");
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }
            
            Thread.Sleep(1000);
        }       
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        entityHive.Delete<SomeValueObject>("ObjectIdentifier");
    }
}
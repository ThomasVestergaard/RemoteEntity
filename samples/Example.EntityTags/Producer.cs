using Microsoft.Extensions.Hosting;
using RemoteEntity;
using RemoteEntity.Tags;

namespace Example.GettingStarted;

public class Producer(IEntityHive entityHive) : IHostedService
{

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var obj = new SomeValueObject
        {
            SomeNumber = 456.55m,
            SomeText = "Update"
        };

        var tags = new List<IEntityTag>();
        tags.Add(EntityTag.Create("bool_tag", true));
        tags.Add(EntityTag.Create("string_tag", "Here is some meta data for your entity"));

        for(int i=0; i<5; i++)
        {
            // Publish 10 messages
            obj.SomeText = $"Update no {i}";
            Console.WriteLine($"PRODUCER: Publishing: '{obj.SomeText}'");

            try
            {
                entityHive.PublishEntity(obj, "ObjectIdentifier", tags);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            Thread.Sleep(1000);
        }

    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        
    }
}
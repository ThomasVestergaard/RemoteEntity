using Microsoft.Extensions.Hosting;
using RemoteEntity;

namespace Example.GettingStarted;

public class Consumer(IEntityHive entityHive) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var observable = entityHive.SubscribeToEntity<SomeValueObject>("ObjectIdentifier", entity =>
        {
            // Handle updates as the come
            Console.WriteLine($"CONSUMER: Update received. Value is: '{entity.SomeText}'");
        });

        observable.OnUpdate += dto =>
        {
            Console.WriteLine($"CONSUMER: Update received. Tags are: ");
            foreach (var tag in observable.Tags)
            {
                Console.WriteLine($"  '{tag.TagName}' ({tag.GetValueType()})='{tag.GetTagValue<object>()}'");
            }
        };

        Console.WriteLine("Consumer. Here are the tags on the entity:");
        foreach (var tag in observable.Tags)
        {
            Console.WriteLine($"  '{tag.TagName}' ({tag.GetValueType()})='{tag.GetTagValue<object>()}'");
        }
        
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        
    }
}
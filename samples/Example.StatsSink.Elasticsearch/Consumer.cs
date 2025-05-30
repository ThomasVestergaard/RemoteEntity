using Microsoft.Extensions.Hosting;
using RemoteEntity;

namespace Example.StatsSink.Elasticsearch;

public class Consumer(IEntityHive entityHive) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var observable = entityHive.SubscribeToEntity<SomeValueObject>("ObjectIdentifier", entity =>
        {
            // Handle updates as the come. Do nothing for now.
        });

        System.Console.WriteLine("Consumer started");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        
    }
}
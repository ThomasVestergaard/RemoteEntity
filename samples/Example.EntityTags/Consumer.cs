using RemoteEntity;

namespace Example.GettingStarted;

public class Consumer(IEntityHive entityHive)
{
    public void Execute()
    {
        entityHive.HiveOptions.PublishDuplicates = false;
        var observable = entityHive.SubscribeToEntity<SomeValueObject>("ObjectIdentifier", entity =>
        {
            
            // Handle updates as the come
            Console.WriteLine($"CONSUMER: Update received. Value is: '{entity.SomeText}'");
        });

        if (observable.Value != null)
        {
            Console.WriteLine($"Entity values when subscribed: '{observable.Value.SomeText}'");
        }
        else
        {
            Console.WriteLine("Entity value when subscribed: null");
        } 
        
            
    }
}
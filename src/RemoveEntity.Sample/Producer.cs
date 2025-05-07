using RemoteEntity;

namespace RemoveEntity.Sample;

public class Producer(IEntityHive entityHive)
{
    public void Execute()
    {
        var obj = new SomeValueObject
        {
            SomeNumber = 456.55m,
            SomeText = "Update"
        };

        for(int i=0; i<5; i++)
        {
            // Publish 10 messages
            obj.SomeText = $"Update no {i}";
            Console.WriteLine($"PRODUCER: Publishing: '{obj.SomeText}'");

            try
            {
                entityHive.PublishEntity(obj, "ObjectIdentifier");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            Thread.Sleep(1000);
        }

        Console.WriteLine($"PRODUCER: Deleting object");
        entityHive.Delete<SomeValueObject>("ObjectIdentifier");
        
    }
}
using RemoteEntity;
namespace RemoveEntity.Sample;

public class SomeValueObject : ICloneable<SomeValueObject>, IInitialSeed<SomeValueObject>
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
}
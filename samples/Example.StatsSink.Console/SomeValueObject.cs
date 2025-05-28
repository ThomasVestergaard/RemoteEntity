using RemoteEntity;

namespace Example.StatsSink.Console;

public class SomeValueObject : ICloneable<SomeValueObject>
{
    public string SomeText { get; set; } = null!;
    public decimal SomeNumber { get; set; }

}
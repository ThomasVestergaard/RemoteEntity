using RemoteEntity;

namespace Example.StatsSink.Elasticsearch;

public class SomeValueObject : ICloneable<SomeValueObject>
{
    public string SomeText { get; set; } = null!;
    public decimal SomeNumber { get; set; }

}
using System.Text.Json;
namespace RemoteEntity;

/// <summary>
/// This is a helper class to make it easier to implement ICloneable.
/// It makes a deep clone by serializing and deserializing the object.
/// This is not fast and may or may not be suited for your needs.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class DeepCloner<T> : ICloneable<T>
{
    public T Clone()
    {
        return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(this))!;
    }
}
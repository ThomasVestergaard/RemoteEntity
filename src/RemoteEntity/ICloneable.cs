using System.Text.Json;

namespace RemoteEntity
{
    public interface ICloneable<T>
    {
        T DeepClone()
        {
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(this, GetType()))!;
        }
    }
}

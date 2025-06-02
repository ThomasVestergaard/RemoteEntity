using System;
using System.Text.Json;

namespace RemoteEntity.Tags;

public class EntityTagDto
{
    public string TagName { get; set; }
    public string? ValueJson { get; set; }
    public string? ValueType { get; set; }

    public static EntityTagDto FromEntityTag(IEntityTag tag)
    {
        return new EntityTagDto
        {
            TagName = tag.TagName,
            ValueType = tag.GetValueType()?.AssemblyQualifiedName,
            ValueJson = JsonSerializer.Serialize(tag.GetTagValue<object>())
        };
    }
    
    public IEntityTag ToEntityTag()
    {
        if (ValueType is null)
            throw new InvalidOperationException("ValueType is required to deserialize.");

        var type = Type.GetType(ValueType)
                   ?? throw new InvalidOperationException($"Cannot resolve type: {ValueType}");

        var value = ValueJson is null
            ? null
            : JsonSerializer.Deserialize(ValueJson, type);

        var method = typeof(EntityTag)
            .GetMethod("Create")!
            .MakeGenericMethod(type);

        return (IEntityTag)method.Invoke(null, new[] { TagName, value })!;
    }
}

using System;

namespace RemoteEntity;

public class EntityTag : IEntityTag
{
    private object? tagValue;
    public string TagName { get; }
    private Type valueType;

    private EntityTag(string tagName, object? value, Type type)
    {
        TagName = tagName;
        tagValue = value;
        valueType = type;
    }
    
    public T? GetTagValue<T>()
    {
        if (tagValue == null)
            return default;
        
        var targetType = typeof(T);
        var valueTypeNonNullable = Nullable.GetUnderlyingType(targetType);

        if (valueTypeNonNullable != null && tagValue.GetType() == valueTypeNonNullable)
        {
            return (T)Convert.ChangeType(tagValue, valueTypeNonNullable);
        }

        if (tagValue is T tValue)
        {
            return tValue;
        }

        throw new InvalidCastException(
            $"Tag value is of type {tagValue.GetType().FullName}, not {typeof(T).FullName}.");
    }
    
    public static EntityTag Create<T>(string tagName, T value)
    {
        var t = typeof(T);
        return new EntityTag(tagName, value, t);
    }
    
    public Type? GetValueType() => valueType;
}
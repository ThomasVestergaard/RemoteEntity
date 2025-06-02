using System;

namespace RemoteEntity.Tags;

public interface IEntityTag
{
    string TagName { get; }
    T? GetTagValue<T>();
    Type? GetValueType();
}
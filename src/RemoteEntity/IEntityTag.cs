using System;

namespace RemoteEntity;

public interface IEntityTag
{
    string TagName { get; }
    T? GetTagValue<T>();
    Type? GetValueType();
}
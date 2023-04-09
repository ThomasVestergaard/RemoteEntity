using System;

namespace RemoteEntity;

public interface IEntityObserver<T> where T : ICloneable<T>
{
    event  EntityObserver<T>.EntityUpdateHandler OnUpdate;
    string EntityId { get; }
    T Value { get; }
    DateTimeOffset PublishTime { get; }
    void Stop();
}
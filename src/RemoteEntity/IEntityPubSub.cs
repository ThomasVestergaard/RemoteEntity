using System;

namespace RemoteEntity
{
    public interface IEntityPubSub
    {
        long Publish<T>(string entityId, EntityDto<T> entity);
        void Subscribe<T>(string entityId, Action<EntityDto<T>> handler);
        void Unsubscribe(string entityId);
        void Start();
    }
}

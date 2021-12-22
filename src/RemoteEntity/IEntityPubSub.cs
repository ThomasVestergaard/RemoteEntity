using System;
using System.Threading.Tasks;

namespace RemoteEntity
{
    public interface IEntityPubSub
    {
        void Publish<T>(string entityId, EntityDto<T> entity);
        void Subscribe<T>(string entityId, Action<EntityDto<T>> handler);
        void Unsubscribe(string entityId);
    }
}

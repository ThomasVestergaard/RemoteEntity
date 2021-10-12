using System;
using System.Threading.Tasks;

namespace RemoteEntity
{
    public interface IEntityPubSub
    {
        void Publish<T>(string channel, EntityDto<T> entity);
        Task Subscribe<T>(string channel, Action<EntityDto<T>> handler);
        void Unsubscribe(string channel);
    }
}

using System;
using System.Threading.Tasks;

namespace RemoteEntity
{
    public interface IEntityHive
    {
        void PublishEntity<T>(T entity, string entityId) where T : ICloneable<T>;
        EntityObserver<T> SubscribeToEntity<T>(string entityId, Action<T> updateHandler) where T : ICloneable<T>;
        Task Stop();
    }
}
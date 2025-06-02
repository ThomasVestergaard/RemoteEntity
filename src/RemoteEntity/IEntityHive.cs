using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RemoteEntity.Tags;

namespace RemoteEntity
{
    public interface IEntityHive
    {
        HiveOptions HiveOptions { get; }
        void PublishEntity<T>(T entity, string entityId) where T : ICloneable<T>;
        void PublishEntity<T>(T entity, string entityId, IEnumerable<IEntityTag> tags) where T : ICloneable<T>;
        IEntityObserver<T> SubscribeToEntity<T>(string entityId, Action<T> updateHandler) where T : ICloneable<T>;
        IEntityObserver<T> SubscribeToEntity<T>(string entityId) where T : ICloneable<T>;
        
        T GetEntity<T>(string entityId) where T : ICloneable<T>;
        Task Start();
        Task Stop();
        void Delete<T>(string entityId);
    }
}
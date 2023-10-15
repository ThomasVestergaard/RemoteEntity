using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RemoteEntity
{
    public class EntityHive : IEntityHive
    {
        private readonly IEntityStorage entityStorage;
        private readonly IEntityPubSub entityPublisher = null!;
        private readonly ILogger logger;
        private List<IManagedObserver> observers { get; set; }
        private List<Task> channelReaderTasks { get; set; }

        public EntityHive(IEntityStorage entityStorage, IEntityPubSub entityPublisher, ILogger<EntityHive> logger)
        {
            this.entityStorage = entityStorage;
            this.entityPublisher = entityPublisher;
            this.logger = logger;
            observers = new List<IManagedObserver>();
            channelReaderTasks = new List<Task>();
        }

        public EntityHive(IEntityStorage entityStorage, ILogger logger)
        {
            this.entityStorage = entityStorage;
            this.logger = logger;
            observers = new List<IManagedObserver>();
            channelReaderTasks = new List<Task>();
        }
        
        public void PublishEntity<T>(T entity, string entityId) where T : ICloneable<T>
        {
            var dto = new EntityDto<T>(entityId, entity, DateTimeOffset.UtcNow);

            // Store state
            if (!entityStorage.ContainsKey(entityId)) //todo seems to be not needed in redis?
            {
                entityStorage.Add(entityId, dto);
            }
            else
            {
                entityStorage.Set(entityId, dto);
            }

            // Publish update
            if (entityPublisher != null)
            {
                entityPublisher.Publish(entityId, dto);
            }
        }

        public IEntityObserver<T> SubscribeToEntity<T>(string entityId) where T : ICloneable<T>
        {
            return SubscribeToEntity<T>(entityId, null!);
        }

        public T GetEntity<T>(string entityId) where T : ICloneable<T>
        {
            if (entityStorage.ContainsKey(entityId))
            {
                var currentEntity = entityStorage.Get<EntityDto<T>>(entityId);
                if (currentEntity != null)
                {
                    return currentEntity.Value;
                }
            }

            return default!;
        }
        
        public IEntityObserver<T> SubscribeToEntity<T>(string entityId, Action<T> updateHandler) where T : ICloneable<T>
        {
            var existingObserver = observers.Find(a => a.EntityId == entityId);
            if (existingObserver != null)
            {
                logger.LogInformation($"Already subscribed to '{entityId}'. Adding update handler");
                var asObserver = (IEntityObserver<T>)existingObserver;

                if (updateHandler != null)
                {
                    asObserver.OnUpdate += value => updateHandler(value);
                }

                return asObserver;
            }
            
            logger.LogInformation($"Subscribing to '{entityId}'");
            var toReturn = new EntityObserver<T>(entityId, logger);

            // Try to get entity data from storage
            if (entityStorage.ContainsKey(entityId))
            {
                var currentEntity = entityStorage.Get<EntityDto<T>>(entityId);
                toReturn.updateValue(currentEntity.Value, currentEntity.PublishTime);
            } else 
            {
                // Create initial seed entity if possible
                var hasInitialSeedEntity = typeof(IInitialSeed<T>).IsAssignableFrom(typeof(T));
                if (hasInitialSeedEntity)
                {
                    var seedEntity = ((IInitialSeed<T>)Activator.CreateInstance(typeof(T))!).InitialSeedEntity();

                    entityStorage.Add(entityId, new EntityDto<T>(entityId, seedEntity, DateTimeOffset.UtcNow));
                    logger.LogInformation($"Added initial seed entity for '{entityId}'");
                    toReturn.updateValue(seedEntity, DateTimeOffset.UtcNow);
                }
            }
            

            if (entityPublisher != null)
            {
                // Channel for updates
                var updateChannel = Channel.CreateUnbounded<EntityDto<T>>();

                // Subscribe to changes
                entityPublisher.Subscribe<T>(entityId, dto => { updateChannel.Writer.TryWrite(dto); });

                var channelReaderTask = toReturn.Start(updateChannel, updateHandler);
                observers.Add(toReturn);
                channelReaderTasks.Add(channelReaderTask);
            }

            return toReturn;
        }

        public void Delete<T>(string entityId)
        {
            logger.LogInformation($"Deleting entity '{entityId}'");
            var observer = observers.Find(a => a.EntityId == entityId);

            entityStorage.Delete(entityId);
            
            if (observer != null)
            {
                // Unsubscribe if subscribed
                entityPublisher.Unsubscribe(entityId);
                observer.Stop();
                observers.Remove(observer);
            }
        }
        
        public Task Stop()
        {
            logger.LogInformation("Stopping EntityHive.");
            return Task.Run(async () =>
            {
                foreach (var entityObserver in observers)
                {
                    entityPublisher.Unsubscribe(entityObserver.EntityId);
                    entityObserver.Stop();
                }

                await Task.WhenAll(channelReaderTasks);
                logger.LogInformation("EntityHive stopped.");
            });
            
        }

    }
}

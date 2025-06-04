using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RemoteEntity.Stats;
using RemoteEntity.Tags;

namespace RemoteEntity
{
    public class EntityHive : IEntityHive
    {
        private readonly IEntityStorage entityStorage;
        private readonly IEntityPubSub? entityPublisher;
        private readonly ILogger logger;
        private readonly IStatsSinkManager statsSinkManager;

        private List<IManagedObserver> observers { get; set; }
        private List<Task> channelReaderTasks { get; set; }
        private IDuplicateDetector duplicateDetector;
        public HiveOptions HiveOptions { get; }
        public EntityHive(IEntityStorage entityStorage, IEntityPubSub entityPublisher, HiveOptions hiveOptions, IDuplicateDetector duplicateDetector, ILogger<EntityHive> logger, IStatsSinkManager statsSinkManager)
        {
            HiveOptions = hiveOptions;
            this.entityStorage = entityStorage;
            this.duplicateDetector = duplicateDetector;
            this.entityPublisher = entityPublisher;
            this.logger = logger;
            this.statsSinkManager = statsSinkManager;

            observers = new List<IManagedObserver>();
            channelReaderTasks = new List<Task>();
        }
  
        public void PublishEntity<T>(T entity, string entityId) where T : ICloneable<T>
        {
            PublishEntity(entity, entityId, new List<IEntityTag>());
        }

        public void PublishEntity<T>(T entity, string entityId, IEnumerable<IEntityTag> tags) where T : ICloneable<T>
        {
            var dto = new EntityDto<T>(entityId, entity, DateTimeOffset.UtcNow, tags);
            var isDuplicate =  duplicateDetector.IsDuplicate(entity, entityId);
            if (isDuplicate && !HiveOptions.PublishDuplicates)
            {
                return;
            }
            
            entityStorage.Set(entityId, dto);
            var publishedBytes = entityPublisher?.Publish(entityId, dto);
            if (publishedBytes != null)
                statsSinkManager.RegisterPublish(entityId, typeof(T).FullName!, publishedBytes.Value);
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
                return currentEntity.Value;
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
                toReturn.updateValue(currentEntity.Value, currentEntity.PublishTime, currentEntity.Tags);
            } else 
            {
                // Create initial seed entity if possible
                var hasInitialSeedEntity = typeof(IInitialSeed<T>).IsAssignableFrom(typeof(T));
                if (hasInitialSeedEntity)
                {
                    var seedEntity = ((IInitialSeed<T>)Activator.CreateInstance(typeof(T))!).InitialSeedEntity();

                    entityStorage.Add(entityId, new EntityDto<T>(entityId, seedEntity, DateTimeOffset.UtcNow, new List<IEntityTag>()));
                    logger.LogInformation($"Added initial seed entity for '{entityId}'");
                    toReturn.updateValue(seedEntity, DateTimeOffset.UtcNow, new List<EntityTagDto>());
                }
            }
            

            if (entityPublisher != null)
            {
                // Channel for updates
                var updateChannel = Channel.CreateUnbounded<EntityDto<T>>();

                // Subscribe to changes
                entityPublisher.Subscribe<T>(entityId, dto =>
                {
                    updateChannel.Writer.TryWrite(dto);
                });

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
                entityPublisher?.Unsubscribe(entityId);
                observer.Stop();
                observers.Remove(observer);
            }
        }

        public async Task Start()
        {
            logger.LogInformation("Starting EntityHive");
            entityPublisher?.Start();
            entityStorage.Start();
            statsSinkManager?.Start();
        }
        
        public Task Stop()
        {
            logger.LogInformation("Stopping EntityHive");
            return Task.Run(async () =>
            {
                foreach (var entityObserver in observers)
                {
                    entityPublisher?.Unsubscribe(entityObserver.EntityId);
                    entityObserver.Stop();
                }

                await Task.WhenAll(channelReaderTasks);
                logger.LogInformation("EntityHive stopped");
            });
            
        }

    }
}

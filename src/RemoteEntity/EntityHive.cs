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
        private readonly IEntityPubSub entityPublisher;
        private readonly ILogger logger;
        private List<IEntityObserver> observers { get; set; }
        private List<Task> channelReaderTasks { get; set; }

        public EntityHive(IEntityStorage entityStorage, IEntityPubSub entityPublisher, ILogger logger)
        {
            this.entityStorage = entityStorage;
            this.entityPublisher = entityPublisher;
            this.logger = logger;
            observers = new List<IEntityObserver>();
            channelReaderTasks = new List<Task>();
        }

        public EntityHive(IEntityStorage entityStorage, ILogger logger)
        {
            this.entityStorage = entityStorage;
            this.logger = logger;
            observers = new List<IEntityObserver>();
            channelReaderTasks = new List<Task>();
        }

        private string getEntityStateKey(string entityId)
        {
            return $"state.{entityId}".ToLower();
        }

        private string getEntityStreamName(string entityId)
        {
            return $"stream.{entityId}".ToLower();
        }
        
        public void PublishEntity<T>(T entity, string entityId) where T : ICloneable<T>
        {
            var dto = new EntityDto<T>(entityId, entity, DateTimeOffset.UtcNow);

            // Store state
            var stateKey = getEntityStateKey(entityId);
            if (!entityStorage.ContainsKey(stateKey))
            {
                entityStorage.Add(stateKey, dto);
            }
            else
            {
                entityStorage.Set(stateKey, dto);
            }

            // Publish update
            if (entityPublisher != null)
            {
                entityPublisher.Publish(getEntityStreamName(entityId), dto);
            }
        }


        public EntityObserver<T> SubscribeToEntity<T>(string entityId, Action<T> updateHandler) where T : ICloneable<T>
        {
            logger.LogInformation($"Subscribing to '{entityId}'");
            var toReturn = new EntityObserver<T>(entityId, logger);

            // Try to get entity data from redis
            if (entityStorage.ContainsKey(getEntityStateKey(entityId)))
            {
                var currentEntity = entityStorage.Get<EntityDto<T>>(getEntityStateKey(entityId));
                toReturn.updateValue(currentEntity.Value, currentEntity.PublishTime);
            }

            if (entityPublisher != null)
            {
                // Channel for updates
                var updateChannel = Channel.CreateUnbounded<EntityDto<T>>();

                // Subscribe to changes
                entityPublisher.Subscribe<T>(getEntityStreamName(entityId), dto => { updateChannel.Writer.TryWrite(dto); });

                var channelReaderTask = toReturn.Start(updateChannel, updateHandler);
                observers.Add(toReturn);
                channelReaderTasks.Add(channelReaderTask);
            }

            return toReturn;
        }

        public Task Stop()
        {
            logger.LogInformation("Stopping EntityHive.");
            return Task.Run(async () =>
            {
                foreach (var entityObserver in observers)
                {
                    entityPublisher.Unsubscribe(getEntityStreamName(entityObserver.EntityId));
                    entityObserver.Stop();
                }

                await Task.WhenAll(channelReaderTasks);
                logger.LogInformation("EntityHive stopped.");
            });
            
        }

    }
}

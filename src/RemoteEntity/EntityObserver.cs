using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RemoteEntity.Tags;

namespace RemoteEntity
{
    public class EntityObserver<T> : IManagedObserver, IEntityObserver<T> where T : ICloneable<T>
    {
        public delegate void EntityUpdateHandler(T newValue);
        public event EntityUpdateHandler OnUpdate = null!;
        private readonly ILogger logger;
        private readonly object lockObject = new object();
        public string EntityId { get; }
        
        private Action<T> updateObserver { get; set; } = null!;
        private Channel<EntityDto<T>> messageChannel { get; set; } = null!;

        public T Value
        {
            get
            {
                lock (lockObject)
                {
                    if (value == null)
                        return default!;

                    return value.DeepClone();
                }
            }
        }
        
        private T value { get; set; } = default!;

        public List<IEntityTag> Tags
        {
            get
            {
                lock (lockObject)
                {
                    return tags;
                }
            }
        }

        private List<IEntityTag> tags { get; set; } = new();

        public DateTimeOffset PublishTime
        {
            get
            {
                lock (lockObject)
                {
                    return new DateTimeOffset(publishTime.DateTime, TimeSpan.Zero);
                }
            }
        }
        private DateTimeOffset publishTime { get; set; }

        protected virtual void RaiseOnUpdateEvent(T newValue)
        {
            OnUpdate?.Invoke(newValue);
        }

        internal EntityObserver(string entityId, ILogger logger)
        {
            this.logger = logger;
            EntityId = entityId;
            publishTime = DateTimeOffset.MinValue;
        }

        internal void updateValue(T newValue, DateTimeOffset publishTime, IEnumerable<EntityTagDto> tags)
        {
            logger.LogTrace($"Update received for '{EntityId}'");
            lock (lockObject)
            {
                value = newValue;
                this.publishTime = publishTime;
            
                this.tags.Clear();
                foreach (var tag in tags)
                    this.tags.Add(tag.ToEntityTag());
            }
            RaiseOnUpdateEvent(value);
        }

        internal Task Start(Channel<EntityDto<T>> messageChannel)
        {
            return Start(messageChannel, null!);
        }
        
        internal Task Start(Channel<EntityDto<T>> messageChannel, Action<T> updateObserver)
        {
            logger.LogTrace($"Starting observer for '{EntityId}'");
            this.updateObserver = updateObserver;
            this.messageChannel = messageChannel;
            return Task.Factory.StartNew(async () => 
                {
                    while (await messageChannel.Reader.WaitToReadAsync())
                    {
                        var message = await messageChannel.Reader.ReadAsync();
                        if (message.EntityId != EntityId)
                            return;

                        if (message.Value != null)
                        {
                            updateValue(message.Value, message.PublishTime, message.Tags);
                        }
                        else
                        {
                            updateValue(default!, DateTimeOffset.MinValue, message.Tags);
                        }

                        if (this.updateObserver != null)
                            updateObserver(Value);
                    }
                },
                TaskCreationOptions.LongRunning
            );
        }

        public void Stop()
        {
            logger.LogTrace($"Stopping observer for '{EntityId}'");
            if (messageChannel != null)
            {
                messageChannel.Writer.Complete();
            }
        }
    }
}
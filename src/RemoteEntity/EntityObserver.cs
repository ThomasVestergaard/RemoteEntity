using System;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RemoteEntity
{
    public class EntityObserver<T> : IEntityObserver where T : ICloneable<T>
    {
        private readonly ILogger logger;
        private object lockObject = new object();
        public string EntityId { get; }
        
        private Action<T> updateObserver { get; set; }
        private Channel<EntityDto<T>> messageChannel { get; set; }
        public T Value
        {
            get
            {
                lock (lockObject)
                {
                    if (value == null)
                        return default;

                    return value.Clone();
                }
            }
        }
        
        private T value { get; set; }

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

        internal EntityObserver(string entityId, ILogger logger)
        {
            this.logger = logger;
            EntityId = entityId;
            publishTime = DateTimeOffset.MinValue;
        }

        internal void updateValue(T newValue, DateTimeOffset publishTime)
        {
            logger.LogTrace($"Update received for '{EntityId}'");
            lock (lockObject)
            {
                value = newValue;
                this.publishTime = publishTime;
            }
        }

        internal Task Start(Channel<EntityDto<T>> messageChannel, Action<T> updateObserver)
        {
            logger.LogTrace($"Starting observer for '{EntityId}'");
            this.updateObserver = updateObserver;
            this.messageChannel = messageChannel;
            return Task.Run(async () =>
            {
                while (await messageChannel.Reader.WaitToReadAsync())
                {
                    var message = await messageChannel.Reader.ReadAsync();
                    if (message.EntityId != EntityId)
                        return;

                    if (message.Value != null)
                    {
                        updateValue(message.Value, message.PublishTime);
                    } else
                    {
                        updateValue(default, DateTimeOffset.MinValue);
                    }

                    if (this.updateObserver != null)
                        updateObserver(Value);
                }
            });
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
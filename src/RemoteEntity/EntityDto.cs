using System;

namespace RemoteEntity
{
    public class EntityDto<T>
    {
        public DateTimeOffset PublishTime { get; set; }
        public string EntityId { get; set; }
        public T Value { get; set; }

        public EntityDto() { }

        public EntityDto(string entityId, T entityValue, DateTimeOffset publishTime)
        {
            PublishTime = publishTime;
            EntityId = entityId;
            Value = entityValue;
        }
    }
}

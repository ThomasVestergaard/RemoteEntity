using System;

namespace RemoteEntity
{
    public class EntityDto<T> : IEntityDto<T>
    {
        public DateTimeOffset PublishTime { get; set; }
        public string EntityId { get; set; } = null!;
        public T Value { get; set; } = default!;

        
        public EntityDto() { }

        public EntityDto(string entityId, T entityValue, DateTimeOffset publishTime)
        {
            PublishTime = publishTime;
            EntityId = entityId;
            Value = entityValue;
        }
    }
}

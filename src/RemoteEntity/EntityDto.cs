using System;
using System.Collections.Generic;
using RemoteEntity.Tags;

namespace RemoteEntity
{
    public class EntityDto<T> : IEntityDto<T>
    {
        public DateTimeOffset PublishTime { get; set; }
        public string EntityId { get; set; } = null!;
        public T Value { get; set; } = default!;

        public List<EntityTag> Tags { get; set; } = new();
        
        public EntityDto() { }

        public EntityDto(string entityId, T entityValue, DateTimeOffset publishTime, List<EntityTag> tags)
        {
            PublishTime = publishTime;
            EntityId = entityId;
            Value = entityValue;
            Tags = tags;
        }
    }
}

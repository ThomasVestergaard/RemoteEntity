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

        public List<EntityTagDto> Tags { get; set; } = new();
        
        public EntityDto() { }

        public EntityDto(string entityId, T entityValue, DateTimeOffset publishTime, IEnumerable<IEntityTag> tags)
        {
            PublishTime = publishTime;
            EntityId = entityId;
            Value = entityValue;
            
            foreach (var tag in tags)
                Tags.Add(EntityTagDto.FromEntityTag(tag));
        }
    }
}

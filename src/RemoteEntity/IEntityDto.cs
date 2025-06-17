using System;
using System.Collections.Generic;
using RemoteEntity.Tags;

namespace RemoteEntity
{
    public interface IEntityDto<T>
    {
        DateTimeOffset PublishTime { get; set; }
        string EntityId { get; set; }
        T Value { get; set; }
        List<EntityTagDto> Tags { get; set; }
    }
}
using System;

namespace RemoteEntity
{
    public interface IEntityDto<T>
    {
        DateTimeOffset PublishTime { get; set; }
        string EntityId { get; set; }
        T Value { get; set; }
    }
}
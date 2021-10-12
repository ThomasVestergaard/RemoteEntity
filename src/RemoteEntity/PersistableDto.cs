using System;

namespace RemoteEntity
{
    public class PersistableDto<T>
    {
        public DateTimeOffset PublishTime { get; set; }
        public T Value { get; set; }
    }
}

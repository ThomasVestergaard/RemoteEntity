namespace RemoteEntity
{
    public class EntityDto<T>
    {
        public string EntityId { get; set; }
        public T? Value { get; set; }

        public EntityDto() { }

        public EntityDto(string entityId, T entityValue)
        {
            EntityId = entityId;
            Value = entityValue;
        }
    }
}

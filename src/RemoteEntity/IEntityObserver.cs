namespace RemoteEntity
{
    public interface IEntityObserver
    {
        string EntityId { get; }
        
        void Stop();
    }
}

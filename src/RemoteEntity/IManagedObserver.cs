namespace RemoteEntity;

public interface IManagedObserver
{
    public void Stop();
    public string EntityId { get; }
}
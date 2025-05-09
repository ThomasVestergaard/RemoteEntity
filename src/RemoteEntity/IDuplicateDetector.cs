namespace RemoteEntity;

public interface IDuplicateDetector
{
    bool IsDuplicate<T>(T entity, string entityId);
}
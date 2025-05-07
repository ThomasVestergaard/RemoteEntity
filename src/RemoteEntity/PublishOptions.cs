namespace RemoteEntity;

public record PublishOptions
{
    public bool PublishDuplicates { get; init; } = true;

    public static PublishOptions Default()
    {
        return new PublishOptions();
    }
}
namespace RemoteEntity.Tags;

public class EntityTag
{
    public string TagName
    {
        get;
        set;
    }

    public string TagValue
    {
        get;
        set;
    }

    public EntityTag()
    {
        
    }

    public EntityTag(string tagName, string tagValue)
    {
        TagName = tagName;
        TagValue = tagValue;
    }
    
}
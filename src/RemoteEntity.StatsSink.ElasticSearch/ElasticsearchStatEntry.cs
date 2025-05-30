namespace RemoteEntity.StatsSink.ElasticSearch;

public class ElasticsearchStatEntry
{
    public string AssemblyName { get; set; }
    public string StatTypeName { get; set; }
    public int StatTypeId { get; set; }
    public string StatKey { get; set; }
    public long StatValue { get; set; }

    
}
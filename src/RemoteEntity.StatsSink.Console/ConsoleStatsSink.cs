using RemoteEntity.Stats;

namespace RemoteEntity.StatsSink.Console;

public class ConsoleStatsSink : IRemoteEntityStatsSink
{
    private StatsCache cache = new();

    public ConsoleStatsSink(ConsoleStatsSinkOptions options)
    {
        
    }
    
    public void ProcessStats(IEnumerable<StatEntry> stats)
    {
        cache.RegisterStats(stats);        
    }
}
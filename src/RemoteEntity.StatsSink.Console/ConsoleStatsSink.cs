using Microsoft.Extensions.Logging;
using RemoteEntity.Stats;
using Timer = System.Timers.Timer;

namespace RemoteEntity.StatsSink.Console;

public class ConsoleStatsSink(ConsoleStatsSinkOptions options, ILogger<ConsoleStatsSink> logger) : IRemoteEntityStatsSink
{
    private StatsCache cache = new();
    private Timer flushTimer;
    public void ProcessStats(IEnumerable<StatEntry> stats)
    {
        cache.RegisterStats(stats);        
    }

    public void Start()
    {
        flushTimer = new Timer(options.FlushToConsoleInterval);
        flushTimer.Elapsed += (_, _) =>
        {
            FlushToConsole();
        };
        flushTimer.Start();

    }

    private void FlushToConsole()
    {
        logger.LogTrace("Flushing remote entity stats to console");

        var stats = cache.Stats;
        if (stats.Keys.Count == 0) return;
        
        System.Console.WriteLine("Remote entity stats:");
        foreach (var stat in stats)
        {
            foreach (var valueKey in stat.Value.Keys)
            {
                System.Console.WriteLine($"{stat.Key.ToString()} ({valueKey}) : {stat.Value[valueKey]}");
            }
        }
        
        cache.Clear();
    }

}
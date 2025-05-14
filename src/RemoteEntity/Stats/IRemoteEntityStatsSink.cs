using System.Collections.Generic;

namespace RemoteEntity.Stats;

public interface IRemoteEntityStatsSink
{
    void ProcessStats(IEnumerable<StatEntry> stats);
    
}
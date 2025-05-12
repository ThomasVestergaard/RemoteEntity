using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemoteEntity.Stats;

public interface IRemoteEntityStatsSink
{
    Task Flush(IEnumerable<StatEntry> stats);
}
using System.Collections.Generic;
using RemoteEntity.Tags;

namespace RemoteEntity;

public interface IInitialSeed<T>
{
    T InitialSeedEntity();
    IEnumerable<IEntityTag> InitialSeedEntityTags();
}
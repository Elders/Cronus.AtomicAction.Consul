using System;
using Elders.Cronus;
using Elders.Cronus.Userfull;

namespace Cronus.AtomicAction.Consul
{
    public interface IRevisionStore
    {
        Result<bool> HasRevision(IAggregateRootId aggregateRootId);

        Result<int> GetRevision(IAggregateRootId aggregateRootId);

        Result<bool> SaveRevision(IAggregateRootId aggregateRootId, int revision, string session);
    }
}

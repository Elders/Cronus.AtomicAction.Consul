using System;
using Elders.Cronus.AtomicAction;

namespace Cronus.AtomicAction.Consul.Tests
{
    public static class TestAtomicActionFactory
    {
        public static IAggregateRootAtomicAction New(ILock aggregateRootLock, IConsulClient conuslClient, IRevisionStore revisionStore)
        {
            return new ConsulAggregateRootAtomicAction(aggregateRootLock, revisionStore, conuslClient, new ConsulAtomicActionOptionsMonitorMock());
        }
    }
}

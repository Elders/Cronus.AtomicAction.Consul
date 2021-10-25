using System;

namespace Cronus.AtomicAction.Consul
{
    public class ConsulAggregateRootAtomicActionOptions
    {
        public TimeSpan LockTtl { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan RevisionTtl { get; set; } = TimeSpan.FromSeconds(10);
    }
}

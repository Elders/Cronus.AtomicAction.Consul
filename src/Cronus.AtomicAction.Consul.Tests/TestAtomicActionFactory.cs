using Elders.Cronus.AtomicAction;

namespace Cronus.AtomicAction.Consul.Tests
{
    public static class TestAtomicActionFactory
    {
        public static IAggregateRootAtomicAction New(IConsulClient conuslClient)
        {
            return new ConsulAggregateRootAtomicAction(conuslClient);
        }
    }
}

using Elders.Cronus;
using Elders.Cronus.AtomicAction;
using FakeItEasy;
using Machine.Specifications;
using Playground;

namespace Cronus.AtomicAction.Consul.Tests.WithLockManager
{
    public abstract class WithLockManagerFailingToAcquireLock
    {
        Establish context = () =>
        {
            arId = new HeadquarterId("20ed0b20-0f7f-4659-9211-0bee5b693e51", "elders");
            sessionId = $"session/{arId}";

            client = A.Fake<IConsulClient>();
            service = TestAtomicActionFactory.New(client);
        };

        protected static AggregateRootId arId;
        protected static string sessionId;
        protected static IConsulClient client;
        protected static IAggregateRootAtomicAction service;
    }
}

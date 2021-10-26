using System;
using Elders.Cronus.AtomicAction;
using FakeItEasy;
using Machine.Specifications;

namespace Cronus.AtomicAction.Consul.Tests.WithLockManager
{
    public abstract class WithLockManagerFailingToAcquireLock
    {
        Establish context = () =>
        {
            lockManager = A.Fake<ILock>();
            client = A.Fake<IConsulClient>();
            revisionStore = A.Fake<IRevisionStore>();
            service = TestAtomicActionFactory.New(lockManager, client, revisionStore);
            A.CallTo(() => lockManager.Lock(A<string>._, A<TimeSpan>._)).Returns(false);
            service = TestAtomicActionFactory.New(lockManager, client, revisionStore);
        };

        protected static ILock lockManager;
        protected static IConsulClient client;
        protected static IRevisionStore revisionStore;
        protected static IAggregateRootAtomicAction service;
    }
}

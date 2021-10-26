using System;
using Elders.Cronus.AtomicAction;
using FakeItEasy;
using Machine.Specifications;

namespace Cronus.AtomicAction.Consul.Tests.WithLockManager
{
    public abstract class WithDramaticLockManager
    {
        Establish context = () =>
        {
            lockManager = A.Fake<ILock>();
            client = A.Fake<IConsulClient>();
            revisionStore = A.Fake<IRevisionStore>();
            A.CallTo(() => lockManager.Lock(A<string>._, A<TimeSpan>._)).Throws(new Exception(message));
            service = TestAtomicActionFactory.New(lockManager, client, revisionStore);
        };

        protected static ILock lockManager;
        protected static IConsulClient client;
        protected static IRevisionStore revisionStore;
        protected static IAggregateRootAtomicAction service;
        protected static string message = "drama";
    }
}

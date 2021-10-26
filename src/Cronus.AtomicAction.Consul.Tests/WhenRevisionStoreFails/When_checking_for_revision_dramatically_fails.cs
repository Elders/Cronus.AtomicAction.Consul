using System;
using Elders.Cronus.AtomicAction;
using Elders.Cronus.Userfull;
using FakeItEasy;
using Machine.Specifications;
using Playground;
using static Cronus.AtomicAction.Consul.ConsulClient;

namespace Cronus.AtomicAction.Consul.Tests.WhenRevisionStoreFails
{
    [Subject("Consul Atomic Action")]
    public class When_checking_for_revision_dramatically_fails
    {
        Establish context = () =>
        {
            id = new HeadquarterId("20ed0b20-0f7f-4659-9211-0bee5b693e51", "elders");
            sessionName = $"session/{id}";
            sessionId = $"session{id}";
            options = new ConsulAtomicActionOptionsMonitorMock().CurrentValue;

            client = A.Fake<IConsulClient>();
            A.CallTo(() => client.CreateSession(sessionName, options.LockTtl, options.RevisionTtl)).Returns(new CreateSessionResponse() { Id = sessionId });

            lockManager = A.Fake<ILock>();
            A.CallTo(() => lockManager.Lock(sessionId, TimeSpan.Zero)).Returns(true);

            revisionStore = A.Fake<IRevisionStore>();
            A.CallTo(() => revisionStore.HasRevision(id)).Throws(new Exception(message));

            options = new ConsulAtomicActionOptionsMonitorMock().CurrentValue;
            service = TestAtomicActionFactory.New(lockManager, client, revisionStore);
        };

        Because of = () => result = service.Execute(id, 1, action);

        It should_return__false__as_a_result = () => result.Value.ShouldBeFalse();
        It should_have_an_exception_recorded = () => result.Errors.ShouldNotBeEmpty();
        It should_not_execute_the_given_action = () => actionExecuted.ShouldBeFalse();
        It should_try_to_create_a_session = () => A.CallTo(() => client.CreateSession(sessionName, options.LockTtl, options.RevisionTtl)).MustHaveHappenedOnceExactly();
        It should_try_to_unlock_the_mutex = () => A.CallTo(() => lockManager.Unlock(sessionId)).MustHaveHappenedOnceExactly();

        It should_not_try_to_persist_the_revision = () =>
            A.CallTo(() => revisionStore.SaveRevision(id, 1, sessionId))
                .MustNotHaveHappened();

        static string message = "drama";
        static string sessionId;
        static string sessionName;
        static HeadquarterId id;
        static ILock lockManager;
        static IConsulClient client;
        static IRevisionStore revisionStore;
        static IAggregateRootAtomicAction service;
        static Result<bool> result;
        static Action action = () => { actionExecuted = true; };
        static bool actionExecuted;
        static ConsulAggregateRootAtomicActionOptions options;
    }
}

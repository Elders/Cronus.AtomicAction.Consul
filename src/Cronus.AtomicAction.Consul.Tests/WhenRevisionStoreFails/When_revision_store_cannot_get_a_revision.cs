using Elders.Cronus.AtomicAction;
using Elders.Cronus.Userfull;
using FakeItEasy;
using System;
using Machine.Specifications;
using Playground;
using static Cronus.AtomicAction.Consul.ConsulClient;

namespace Cronus.AtomicAction.Consul.Tests.WhenRevisionStoreFails
{
    [Subject("Consul Atomic Action")]
    public class When_revision_store_cannot_get_a_revision
    {
        Establish context = () =>
        {
            id = new HeadquarterId("20ed0b20-0f7f-4659-9211-0bee5b693e51", "elders");
            sessionName = $"session/{id}";
            sessionId = $"session{id}";
            options = new ConsulAtomicActionOptionsMonitorMock().CurrentValue;

            lockManager = A.Fake<ILock>();
            A.CallTo(() => lockManager.Lock(sessionId, A<TimeSpan>.Ignored)).Returns(true);

            client = A.Fake<IConsulClient>();
            A.CallTo(() => client.CreateSession(sessionName, options.LockTtl, options.RevisionTtl)).Returns(new CreateSessionResponse() { Id = sessionId });

            revisionStore = A.Fake<IRevisionStore>();
            A.CallTo(() => revisionStore.HasRevision(id)).Returns(Elders.Cronus.Userfull.Result.Success);
            A.CallTo(() => revisionStore.GetRevision(id)).Throws(new Exception(message));
            service = TestAtomicActionFactory.New(lockManager, client, revisionStore);
        };

        Because of = () => result = service.Execute(id, 1, action);

        It should_return_false_as_a_result = () => result.Value.ShouldBeFalse();
        It should_have_an_exception_recorded = () => result.Errors.ShouldNotBeEmpty();
        It should_not_execute_the_given_action = () => actionExecuted.ShouldBeFalse();
        It should_try_to_unlock_the_mutex = () => A.CallTo(() => lockManager.Unlock(sessionId)).MustHaveHappened();
        It should_not_try_to_persist_the_revision_for_a_long_period = () =>
            A.CallTo(() => revisionStore.SaveRevision(id, 1, sessionId))
                .MustNotHaveHappened();

        static string message = "cannot get revision";
        static HeadquarterId id;
        static string sessionId;
        static string sessionName;
        static IConsulClient client;
        static ILock lockManager;
        static IRevisionStore revisionStore;
        static IAggregateRootAtomicAction service;
        static Result<bool> result;
        static Action action = () => { actionExecuted = true; };
        static bool actionExecuted = false;
        static ConsulAggregateRootAtomicActionOptions options;
    }
}

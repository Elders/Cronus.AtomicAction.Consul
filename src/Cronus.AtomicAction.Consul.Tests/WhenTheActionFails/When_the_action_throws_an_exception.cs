using System;
using Elders.Cronus.AtomicAction;
using Elders.Cronus.Userfull;
using FakeItEasy;
using Machine.Specifications;
using Playground;

namespace Cronus.AtomicAction.Consul.Tests.WhenTheActionFails
{
    [Subject("Consul Atomic Action")]
    public class When_the_action_throws_an_exception
    {
        Establish context = () =>
        {
            id = new HeadquarterId("20ed0b20-0f7f-4659-9211-0bee5b693e51", "elders");
            sessionName = $"session/{id}";
            sessionId = $"session{id}";
            options = new ConsulAtomicActionOptionsMonitorMock().CurrentValue;

            client = A.Fake<IConsulClient>();
            A.CallTo(() => client.CreateSession(sessionName)).Returns(new CreateSessionResponse() { Id = sessionId });

            revisionStore = A.Fake<IRevisionStore>();
            A.CallTo(() => revisionStore.HasRevision(id)).Returns(Elders.Cronus.Userfull.Result.Success);
            A.CallTo(() => revisionStore.GetRevision(id)).Returns(new Result<int>(1));
            A.CallTo(() => revisionStore.SaveRevision(id, 2, sessionId)).Returns(new Result<bool>(true));

            lockManager = A.Fake<ILock>();
            A.CallTo(() => lockManager.Lock(sessionId, A<TimeSpan>.Ignored)).Returns(true);

            options = new ConsulAtomicActionOptionsMonitorMock().CurrentValue;
            service = TestAtomicActionFactory.New(lockManager, client, revisionStore);
        };

        Because of = () => result = service.Execute(id, 2, action);

        It should_return_false_as_a_result = () => result.Value.ShouldBeFalse();
        It should_have_an_exception_produced = () => result.Errors.ShouldNotBeEmpty();

        It should_try_to_increment_the_stored_revision = () =>
            A.CallTo(() => revisionStore.SaveRevision(id, 2, sessionId))
                .MustHaveHappened();
        It should_try_to_decrement_the_stored_revision = () =>
            A.CallTo(() => revisionStore.SaveRevision(id, 1, sessionId))
                .MustHaveHappened();

        It should_execute_the_given_action = () => actionExecuted.ShouldBeTrue();
        It should_try_to_unlock_the_mutex = () => A.CallTo(() => lockManager.Unlock(sessionId)).MustHaveHappened();

        static HeadquarterId id;
        static string sessionId;
        static string sessionName;
        static IConsulClient client;
        static ILock lockManager;
        static IRevisionStore revisionStore;
        static Result<bool> result;
        static IAggregateRootAtomicAction service;
        static bool actionExecuted = false;
        static ConsulAggregateRootAtomicActionOptions options;
        static Action action = () =>
        {
            actionExecuted = true;
            throw new Exception();
        };
    }
}

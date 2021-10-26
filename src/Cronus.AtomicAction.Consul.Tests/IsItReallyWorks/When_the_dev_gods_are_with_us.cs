using System;
using Elders.Cronus.AtomicAction;
using Elders.Cronus.Userfull;
using FakeItEasy;
using Machine.Specifications;
using Playground;
using static Cronus.AtomicAction.Consul.ConsulClient;

namespace Cronus.AtomicAction.Consul.Tests.IsItReallyWorks
{
    [Subject("Consul Atomic Action")]
    public class When_the_dev_gods_are_with_us
    {
        Establish context = () =>
        {
            id = new HeadquarterId("20ed0b20-0f7f-4659-9211-0bee5b693e51", "elders");
            sessionName = $"session/{id}";
            sessionId = $"session{id}";
            options = new ConsulAtomicActionOptionsMonitorMock().CurrentValue;
            clientOptions = new ConsulClientOptionsMonitorMock().CurrentValue;

            // Fakes
            client = A.Fake<IConsulClient>();
            lockManager = A.Fake<ILock>();
            revisionStore = A.Fake<IRevisionStore>();
            A.CallTo(() => client.CreateSession(sessionName, options.LockTtl, options.RevisionTtl)).Returns(new CreateSessionResponse() { Id = sessionId });
            A.CallTo(() => lockManager.Lock(sessionId, TimeSpan.Zero)).Returns(true);
            A.CallTo(() => revisionStore.SaveRevision(id, revision, sessionId)).Returns(new Result<bool>(true));
            service = TestAtomicActionFactory.New(lockManager, client, revisionStore);
        };

        Because of = () => result = service.Execute(id, revision, action);

        It should_return_true_as_a_result = () => result.IsSuccessful.ShouldBeTrue();

        It should_not_have_an_exception_recorded = () => result.Errors.ShouldBeEmpty();

        It should_try_to_create_a_session = () => A.CallTo(() => client.CreateSession(sessionName, options.LockTtl, options.RevisionTtl)).MustHaveHappenedOnceExactly();

        It should_lock_the_mutex = () => A.CallTo(() => lockManager.Lock(sessionId, TimeSpan.Zero)).MustHaveHappenedOnceExactly();

        It should_try_to_unlock_the_mutex = () => A.CallTo(() => lockManager.Unlock(sessionId)).MustHaveHappenedOnceExactly();

        It should_try_to_persist_the_stored_revision = () => A.CallTo(() => revisionStore.SaveRevision(id, revision, sessionId)).MustHaveHappened();

        It should_execute_the_given_action = () => actionExecuted.ShouldBeTrue();

        static int revision = 1;
        static string sessionName;
        static string sessionId;
        static HeadquarterId id;
        static ILock lockManager;
        static IRevisionStore revisionStore;
        static IAggregateRootAtomicAction service;
        static IConsulClient client;
        static Result<bool> result;
        static Action action = () => { actionExecuted = true; };
        static bool actionExecuted = false;
        static ConsulAggregateRootAtomicActionOptions options;
        static ConsulClientOptions clientOptions;
    }
}

// Real Dependencies
//client = TestConsulClientFactory.New();
//lockManager = new ConsulLock(client);
//revisionStore = new ConsulRevisionStore(client);
//service = TestAtomicActionFactory.New(lockManager, client, revisionStore);


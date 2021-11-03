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
            arId = new HeadquarterId("20ed0b20-0f7f-4659-9211-0bee5b693e51", "elders");
            sessionName = $"cronus/{arId.NID}/{Convert.ToBase64String(arId.RawId)}";
            sessionId = $"session/{arId}";
            client = A.Fake<IConsulClient>();
            A.CallTo(() => client.CreateSession(sessionName)).Returns(new CreateSessionResponse() { Id = sessionId });
            A.CallTo(() => client.CreateKeyValue(sessionName, revision, sessionId)).Returns(true);
            service = TestAtomicActionFactory.New(client);
        };

        Because of = () => result = service.Execute(arId, 1, action);

        It should_return_false_as_a_result = () => result.Value.ShouldBeFalse();
        It should_have_an_exception_produced = () => result.Errors.ShouldNotBeEmpty();
        It should_execute_the_given_action = () => actionExecuted.ShouldBeTrue();
        It should_try_to_create_a_session = () => A.CallTo(() => client.CreateSession(sessionName)).MustHaveHappened();
        It should_try_to_create_a_kv = () => A.CallTo(() => client.CreateKeyValue(sessionName, revision, sessionId)).MustHaveHappened();
        It should_try_to_unlock_the_mutex = () => A.CallTo(() => client.DeleteSessionAsync(sessionId)).MustHaveHappened();

        static int revision = 1;
        static HeadquarterId arId;
        static string sessionId;
        static string sessionName;
        static IConsulClient client;
        static Result<bool> result;
        static IAggregateRootAtomicAction service;
        static bool actionExecuted = false;
        static Action action = () =>
        {
            actionExecuted = true;
            throw new Exception();
        };
    }
}

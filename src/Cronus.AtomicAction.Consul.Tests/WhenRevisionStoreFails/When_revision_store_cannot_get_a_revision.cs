using Elders.Cronus.AtomicAction;
using Elders.Cronus.Userfull;
using FakeItEasy;
using System;
using Machine.Specifications;
using Playground;

namespace Cronus.AtomicAction.Consul.Tests.WhenRevisionStoreFails
{
    [Subject("Consul Atomic Action")]
    public class When_revision_store_cannot_get_a_revision
    {
        Establish context = () =>
        {
            arId = new HeadquarterId("20ed0b20-0f7f-4659-9211-0bee5b693e51", "elders");
            sessionName = $"cronus/{arId.NID}/{Convert.ToBase64String(arId.RawId)}";
            sessionId = $"session/{arId}";
            client = A.Fake<IConsulClient>();
            A.CallTo(() => client.CreateSession(sessionName)).Returns(new CreateSessionResponse() { Id = sessionId });
            service = TestAtomicActionFactory.New(client);
        };

        Because of = () => result = service.Execute(arId, 1, action);

        It should_return_false_as_a_result = () => result.Value.ShouldBeFalse();
        It should_have_an_exception_recorded = () => result.Errors.ShouldNotBeEmpty();
        It should_not_execute_the_given_action = () => actionExecuted.ShouldBeFalse();
        It should_try_to_unlock_the_mutex = () => A.CallTo(() => client.DeleteSessionAsync(sessionId)).MustHaveHappened();

        static HeadquarterId arId;
        static string sessionId;
        static string sessionName;
        static IConsulClient client;
        static IAggregateRootAtomicAction service;
        static Result<bool> result;
        static Action action = () => { actionExecuted = true; };
        static bool actionExecuted = false;
    }
}

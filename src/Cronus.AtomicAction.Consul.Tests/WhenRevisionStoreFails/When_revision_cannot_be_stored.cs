using System;
using System.Threading.Tasks;
using Elders.Cronus.AtomicAction;
using Elders.Cronus.Userfull;
using FakeItEasy;
using Machine.Specifications;
using Playground;

namespace Cronus.AtomicAction.Consul.Tests.WhenRevisionStoreFails
{
    [Subject("Consul Atomic Action")]
    public class When_revision_cannot_be_stored
    {
        Establish context = () =>
        {
            arId = new HeadquarterId("20ed0b20-0f7f-4659-9211-0bee5b693e51", "elders");
            sessionName = $"cronus/{arId.NID}/{Convert.ToBase64String(arId.RawId)}";
            sessionId = $"session/{arId}";
            client = A.Fake<IConsulClient>();
            A.CallTo(() => client.CreateSessionAsync(sessionName)).Returns(new CreateSessionResponse() { Id = sessionId });
            service = TestAtomicActionFactory.New(client);
        };

        Because of = async () => result = await service.ExecuteAsync(arId, 1, () => { return Task.CompletedTask; });

        It should_return__false__as_a_result = () => result.Value.ShouldBeFalse();
        It should_have_an_exception_recorded = () => result.Errors.ShouldNotBeEmpty();
        It should_try_to_unlock_the_mutex = () => A.CallTo(() => client.DeleteSessionAsync(sessionId)).MustHaveHappenedOnceExactly();
        It should_not_execute_the_given_action = () => actionExecuted.ShouldBeFalse();

        static string sessionId;
        static string sessionName;
        static HeadquarterId arId;
        static IConsulClient client;
        static IAggregateRootAtomicAction service;
        static Result<bool> result;
        static Func<Task> action = () => { actionExecuted = true; return Task.CompletedTask; };
        static bool actionExecuted;
    }
}

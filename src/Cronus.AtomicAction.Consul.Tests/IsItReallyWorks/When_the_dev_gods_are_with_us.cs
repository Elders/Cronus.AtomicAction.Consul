using System;
using System.Threading.Tasks;
using Elders.Cronus.AtomicAction;
using Elders.Cronus.Userfull;
using FakeItEasy;
using Machine.Specifications;
using Playground;

namespace Cronus.AtomicAction.Consul.Tests.IsItReallyWorks
{
    [Subject("Consul Atomic Action")]
    public class When_the_dev_gods_are_with_us
    {
        Establish context = () =>
        {
            arId = new HeadquarterId("20ed0b20-0f7f-4659-9211-0bee5b693e51", "elders");
            sessionName = $"cronus/{arId.NID}/{Convert.ToBase64String(arId.RawId)}";
            sessionId = $"session/{arId}";

            // Fakes
            client = A.Fake<IConsulClient>();
            A.CallTo(() => client.CreateSessionAsync(sessionName)).Returns(new CreateSessionResponse() { Id = sessionId });
            A.CallTo(() => client.CreateKeyValueAsync(sessionName, revision, sessionId)).Returns(true);
            service = TestAtomicActionFactory.New(client);
        };

        Because of = async () => result = await service.ExecuteAsync(arId, revision, action);

        It should_return_true_as_a_result = () => result.IsSuccessful.ShouldBeTrue();

        It should_not_have_an_exception_recorded = () => result.Errors.ShouldBeEmpty();

        It should_try_to_create_a_session = () => A.CallTo(() => client.CreateSessionAsync(sessionName)).MustHaveHappenedOnceExactly();

        It should_try_to_create_a_kv = () => A.CallTo(() => client.CreateKeyValueAsync(sessionName, revision, sessionId)).MustHaveHappenedOnceExactly();

        It should_try_to_delete_session = () => A.CallTo(() => client.DeleteSessionAsync(sessionId)).MustHaveHappenedOnceExactly();

        It should_execute_the_given_action = () => actionExecuted.ShouldBeTrue();

        static int revision = 1;
        static string sessionName;
        static string sessionId;
        static HeadquarterId arId;
        static IAggregateRootAtomicAction service;
        static IConsulClient client;
        static Result<bool> result;
        static Func<Task> action = () => { actionExecuted = true; return Task.CompletedTask; };
        static bool actionExecuted = false;
    }
}


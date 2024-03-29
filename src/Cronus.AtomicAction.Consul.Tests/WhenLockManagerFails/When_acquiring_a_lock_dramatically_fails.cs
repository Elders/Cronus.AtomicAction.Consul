﻿using System;
using System.Threading.Tasks;
using Cronus.AtomicAction.Consul.Tests.WithLockManager;
using Elders.Cronus;
using Elders.Cronus.Userfull;
using FakeItEasy;
using Machine.Specifications;

namespace Cronus.AtomicAction.Consul.Tests.WhenLockManagerFails
{
    [Subject("Consul Atomic Action")]
    public class When_acquiring_a_lock_dramatically_fails : WithDramaticLockManager
    {
        Because of = async () => result = await service.ExecuteAsync(A.Fake<IAggregateRootId>(), 1, action);

        It should_return__false__as_a_result = () => result.IsSuccessful.ShouldBeFalse();
        It should_have_exception_recorded = () => result.Errors.ShouldNotBeEmpty();
        It should_not_execute_the_given_action = () => actionExecuted.ShouldBeFalse();
        It should_not_try_to_unlock_the_mutex = () => A.CallTo(() => client.DeleteSessionAsync(sessionId)).MustNotHaveHappened();

        static Result<bool> result;
        static Func<Task> action = () => { actionExecuted = true; return Task.CompletedTask; };
        static bool actionExecuted = false;
    }
}

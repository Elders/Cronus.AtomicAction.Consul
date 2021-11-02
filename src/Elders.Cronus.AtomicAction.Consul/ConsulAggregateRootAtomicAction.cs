﻿using System;
using Elders.Cronus;
using Elders.Cronus.AtomicAction;
using Elders.Cronus.Userfull;
using Microsoft.Extensions.Logging;

namespace Cronus.AtomicAction.Consul
{
    internal class ConsulAggregateRootAtomicAction : IAggregateRootAtomicAction
    {
        private static readonly ILogger logger = CronusLogger.CreateLogger<ConsulAggregateRootAtomicAction>();
        private IConsulClient consul;

        public ConsulAggregateRootAtomicAction(IConsulClient consulClient)
        {
            this.consul = consulClient;
        }

        public Result<bool> Execute(IAggregateRootId arId, int aggregateRootRevision, Action action)
        {
            string id = Convert.ToBase64String(arId.RawId);
            string sessionName = $"cronus/{arId.NID}/{id}";

            CreateSessionResponse session = null;
            try
            {
                session = consul.CreateSession(sessionName);

                if (session.Success == false)
                    return Result.Error($"Unable to create consul session for {id}");

                Result<bool> lockResult = PersistRevisionWith(arId, aggregateRootRevision, session.Id);

                if (lockResult.IsSuccessful)
                {
                    Result<bool> actionResult = ExecuteAction(action);
                    if (actionResult.IsSuccessful)
                    {
                        return actionResult;
                    }
                    else
                    {
                        return Result.Error($"AtomicAction failed becouse of: {actionResult.Errors?.MakeJustOneException()}");
                    }
                }

                return new Result<bool>(false).WithError($"Unable to execute action for {id}");
            }
            catch (Exception ex)
            {
                logger.ErrorException($"Unable to execute action for {id}", ex);
                return Result.Error(ex);
            }
            finally
            {
                Unlock(session?.Id);
            }
        }

        private void Unlock(string resource)
        {
            try
            {
                consul.DeleteSessionAsync(resource);
            }
            catch (Exception ex)
            {
                logger.WarnException($"Unable to release lock for resource '{resource}' explicitly. The lock will be released automatically.", ex);
            }
        }

        private Result<bool> PersistRevisionWith(IAggregateRootId arId, int revision, string session)
        {
            var revisionKey = GetRevisionKey(arId);
            var created = consul.CreateKeyValueAsync(new CreateKeyValueRequest(revisionKey, revision, session)).ConfigureAwait(false).GetAwaiter().GetResult();
            if (created == false)
                return new Result<bool>(created).WithError("Unable to obtain lock for");

            return new Result<bool>(created);
        }

        private Result<bool> ExecuteAction(Action action)
        {
            try
            {
                action();
                return Result.Success;
            }
            catch (Exception ex)
            {
                return Result.Error(ex);
            }
        }

        private string GetRevisionKey(IAggregateRootId aggregateRootId) => $"cronus/{aggregateRootId.NID}/{Convert.ToBase64String(aggregateRootId.RawId)}";

        public void Dispose()
        {
            // There is a TTL set to every key inserted in Consul so there is no need for explicit releasing of locks
        }
    }
}

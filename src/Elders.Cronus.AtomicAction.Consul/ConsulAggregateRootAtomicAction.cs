using System;
using System.Threading.Tasks;
using Elders.Cronus;
using Elders.Cronus.AtomicAction;
using Elders.Cronus.Userfull;
using Microsoft.Extensions.Logging;

namespace Cronus.AtomicAction.Consul
{
    internal class ConsulAggregateRootAtomicAction : IAggregateRootAtomicAction
    {
        private readonly ILogger logger;
        private IConsulClient consul;

        public ConsulAggregateRootAtomicAction(IConsulClient consulClient, ILogger<ConsulAggregateRootAtomicAction> logger = null)
        {
            this.logger = logger;
            this.consul = consulClient;
        }

        public async Task<Result<bool>> ExecuteAsync(IAggregateRootId arId, int aggregateRootRevision, Func<Task> action)
        {
            string id = Convert.ToBase64String(arId.RawId);
            string sessionName = $"cronus/{arId.NID}/{id}";

            CreateSessionResponse session = null;
            try
            {
                session = await consul.CreateSessionAsync(sessionName).ConfigureAwait(false);

                if (session.Success == false)
                    return Result.Error($"Unable to create consul session for {id}");

                Result<bool> lockResult = await PersistRevisionWithAsync(arId, aggregateRootRevision, session.Id).ConfigureAwait(false);

                if (lockResult.IsSuccessful)
                {
                    Result<bool> actionResult = await ExecuteActionAsync(action).ConfigureAwait(false);
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
            catch (Exception ex) when (logger.ErrorException(ex, () => $"Unable to execute action for {id}"))
            {
                return Result.Error(ex);
            }
            finally
            {
                await UnlockAsync(session?.Id).ConfigureAwait(false);
            }
        }

        private async Task UnlockAsync(string resource)
        {
            try
            {
                await consul.DeleteSessionAsync(resource).ConfigureAwait(false);
            }
            catch (Exception ex) when (logger.WarnException(ex, () => $"Unable to release lock for resource '{resource}' explicitly. The lock will be released automatically.")) { }
        }

        private async Task<Result<bool>> PersistRevisionWithAsync(IAggregateRootId arId, int revision, string session)
        {
            string revisionKey = GetRevisionKey(arId);
            bool created = await consul.CreateKeyValueAsync(revisionKey, revision, session).ConfigureAwait(false);
            if (created == false)
                return new Result<bool>(created).WithError("Unable to obtain lock for");

            return new Result<bool>(created);
        }

        private async Task<Result<bool>> ExecuteActionAsync(Func<Task> action)
        {
            try
            {
                await action().ConfigureAwait(false);
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

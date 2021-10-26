using System;
using Elders.Cronus;
using Elders.Cronus.AtomicAction;
using Elders.Cronus.Userfull;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static Cronus.AtomicAction.Consul.ConsulClient;

namespace Cronus.AtomicAction.Consul
{
    public class ConsulAggregateRootAtomicAction : IAggregateRootAtomicAction
    {
        private static readonly ILogger logger = CronusLogger.CreateLogger<ConsulAggregateRootAtomicAction>();

        private ILock aggregateRootLock;
        private IRevisionStore revisionStore;
        private IConsulClient consulClient;
        private ConsulAggregateRootAtomicActionOptions options;

        public ConsulAggregateRootAtomicAction(ILock aggregateRootLock, IRevisionStore revisionStore, IConsulClient consulClient, IOptionsMonitor<ConsulAggregateRootAtomicActionOptions> options)
        {
            this.aggregateRootLock = aggregateRootLock;
            this.revisionStore = revisionStore;
            this.consulClient = consulClient;
            this.options = options.CurrentValue;
            options.OnChange(newOptions => this.options = newOptions);
        }

        public Result<bool> Execute(IAggregateRootId arId, int aggregateRootRevision, Action action)
        {
            var sessionName = $"session/{arId}";
            var session = consulClient.CreateSession(sessionName, options.LockTtl, options.RevisionTtl);
            if (session.Success == false)
                return Result.Error("Unable to create session");

            var lockResult = Lock(arId, session.Id);
            if (lockResult.IsNotSuccessful)
                return Result.Error($"Lock failed becouse of: {lockResult.Errors?.MakeJustOneException()}");

            try
            {
                var canExecuteActionResult = CanExecuteAction(arId, aggregateRootRevision, session.Id);
                if (canExecuteActionResult.IsSuccessful && canExecuteActionResult.Value)
                {
                    var actionResult = ExecuteAction(action);

                    if (actionResult.IsNotSuccessful)
                    {
                        Rollback(arId, aggregateRootRevision, session.Id);
                        return Result.Error($"Action failed becouse of: {actionResult.Errors?.MakeJustOneException()}");
                    }

                    PersistRevision(arId, aggregateRootRevision, session.Id);

                    return actionResult;
                }

                return new Result<bool>(false).WithError("Unable to execute action").WithError(canExecuteActionResult.Errors?.MakeJustOneException());
            }
            catch (Exception ex)
            {
                logger.ErrorException("Unable to execute action", ex);
                return Result.Error(ex);
            }
            finally
            {
                if (session.Success)
                    Unlock(session.Id);
            }
        }

        public void Dispose()
        {
            (aggregateRootLock as IDisposable)?.Dispose();
            aggregateRootLock = null;

            (revisionStore as IDisposable)?.Dispose();
            revisionStore = null;

            (consulClient as IDisposable)?.Dispose();
            consulClient = null;
        }

        private Result<string> Lock(IAggregateRootId arId, string resource)
        {
            try
            {
                if (aggregateRootLock.Lock(resource, TimeSpan.Zero) == false)
                    return new Result<string>().WithError($"Failed to lock aggregate with id: {arId.Value}");

                return new Result<string>(resource);
            }
            catch (Exception ex)
            {
                return new Result<string>().WithError(ex);
            }
        }

        private Result<bool> CheckForExistingRevision(IAggregateRootId arId)
        {
            return revisionStore.HasRevision(arId);
        }

        private Result<bool> SavePreviouseRevison(IAggregateRootId arId, int revision, string session)
        {
            return revisionStore.SaveRevision(arId, revision - 1, session);
        }

        private Result<bool> PersistRevision(IAggregateRootId arId, int revision, string session)
        {
            return revisionStore.SaveRevision(arId, revision, session);
        }

        private bool IsConsecutiveRevision(IAggregateRootId arId, int revision)
        {
            var storedRevisionResult = revisionStore.GetRevision(arId);
            return storedRevisionResult.IsSuccessful && storedRevisionResult.Value == revision - 1;
        }

        private Result<bool> IncrementRevision(IAggregateRootId arId, int newRevision, string session)
        {
            return revisionStore.SaveRevision(arId, newRevision, session);
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

        private void Rollback(IAggregateRootId arId, int revision, string session)
        {
            revisionStore.SaveRevision(arId, revision - 1, session);
        }

        private void Unlock(string resource)
        {
            if (string.IsNullOrEmpty(resource)) return;

            try
            {
                aggregateRootLock.Unlock(resource);
            }
            catch (Exception ex)
            {
                logger.ErrorException("Unable to unlock", ex);
            }
        }

        Result<bool> CanExecuteAction(IAggregateRootId arId, int aggregateRootRevision, string session)
        {
            try
            {
                var existingRevisionResult = CheckForExistingRevision(arId);
                if (existingRevisionResult.IsNotSuccessful)
                {
                    return new Result<bool>(false).WithError("ExistingRevisionResult is false.").WithError(existingRevisionResult.Errors?.MakeJustOneException());
                }

                if (existingRevisionResult.Value == false)
                {
                    var prevRevResult = SavePreviouseRevison(arId, aggregateRootRevision, session);

                    if (prevRevResult.IsNotSuccessful)
                        return new Result<bool>(false).WithError("PrevRevResult is false.").WithError(prevRevResult.Errors?.MakeJustOneException());
                }

                var isConsecutiveRevision = IsConsecutiveRevision(arId, aggregateRootRevision);
                if (isConsecutiveRevision)
                {
                    return IncrementRevision(arId, aggregateRootRevision, session);
                }

                return new Result<bool>(false).WithError("Revisions were not consecutive");
            }
            catch (Exception ex)
            {
                return new Result<bool>(false).WithError(ex);
            }
        }
    }
}

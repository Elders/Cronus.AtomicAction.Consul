using System;
using System.Linq;
using System.Text;
using Elders.Cronus;
using Elders.Cronus.Userfull;

namespace Cronus.AtomicAction.Consul
{
    public class ConsulRevisionStore : IRevisionStore, IDisposable
    {
        private IConsulClient client;

        public ConsulRevisionStore(IConsulClient client)
        {
            this.client = client;
        }

        public Result<int> GetRevision(IAggregateRootId aggregateRootId)
        {
            var key = GetRevisionKey(aggregateRootId);
            var readKeyValues = client.ReadKeyValueAsync(key).GetAwaiter().GetResult();
            var found = readKeyValues.FirstOrDefault(x => x.Key == key);
            if (found is null)
                return new Result<int>().WithError($"Revision for aggregate root '{aggregateRootId.Value}' was not found.");

            var bytes = Convert.FromBase64String(found.Value);
            var revision = int.Parse(Encoding.UTF8.GetString(bytes, 0, bytes.Length));

            return new Result<int>(revision);
        }

        public Result<bool> HasRevision(IAggregateRootId aggregateRootId)
        {
            var key = GetRevisionKey(aggregateRootId);
            var readKeyValues = client.ReadKeyValueAsync(key).GetAwaiter().GetResult(); ;
            var found = readKeyValues.Any(x => x.Key == key);

            return new Result<bool>(found);
        }

        public Result<bool> SaveRevision(IAggregateRootId aggregateRootId, int revision, string session)
        {
            var revisionKey = GetRevisionKey(aggregateRootId);
            var created = client.CreateKeyValueAsync(new CreateKeyValueRequest(revisionKey, revision, session)).GetAwaiter().GetResult(); ;
            return new Result<bool>(created);
        }

        public void Dispose()
        {
            (client as IDisposable)?.Dispose();
            client = null;
        }

        private string GetRevisionKey(IAggregateRootId aggregateRootId) => $"cronus/revision/{Convert.ToBase64String(aggregateRootId.RawId)}";
    }
}

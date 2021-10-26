using System;
using System.Linq;
using Elders.Cronus;
using Elders.Cronus.AtomicAction;
using Microsoft.Extensions.Logging;

namespace Cronus.AtomicAction.Consul
{
    public class ConsulLock : ILock, IDisposable
    {
        private static readonly ILogger logger = CronusLogger.CreateLogger<ConsulLock>();

        private IConsulClient client;

        public ConsulLock(IConsulClient client)
        {
            this.client = client;
        }

        public bool IsLocked(string resource)
        {
            if (string.IsNullOrEmpty(resource)) throw new ArgumentException(nameof(resource));

            try
            {
                var response = client.ReadKeyValueAsync(resource).Result;
                return response.Any(x => x.Key == resource);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Unable to determine if resource '{resource}' is locked");
                return false;
            }
        }

        public bool Lock(string resource, TimeSpan ttl)
        {
            if (string.IsNullOrEmpty(resource)) throw new ArgumentException(nameof(resource));

            try
            {
                var keyValueResponse = client.CreateKeyValueAsync(new ConsulClient.CreateKeyValueRequest(resource, null, resource)).Result;
                return keyValueResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Unable to acquire lock for resource '{resource}'");
                return false;
            }
        }

        public void Unlock(string resource)
        {
            try
            {
                client.DeleteSessionAsync(resource).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Unable to release lock for resource '{resource}'");
            }
        }

        public void Dispose()
        {
            (client as IDisposable)?.Dispose();
            client = null;
        }
    }
}

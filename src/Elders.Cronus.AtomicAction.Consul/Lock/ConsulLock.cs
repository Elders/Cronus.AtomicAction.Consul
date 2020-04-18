using System;
using System.Linq;
using Elders.Cronus;
using Elders.Cronus.AtomicAction;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cronus.AtomicAction.Consul
{
    public class ConsulLock : ILock
    {
        private static readonly ILogger logger = CronusLogger.CreateLogger<ConsulLock>();

        private readonly IConsulClient client;
        private ConsulLockOptions options;

        public ConsulLock(IConsulClient client, IOptionsMonitor<ConsulLockOptions> options)
        {
            this.client = client;
            this.options = options.CurrentValue;
            options.OnChange(newOptions => this.options = newOptions);
        }

        public bool IsLocked(string resource)
        {
            if (string.IsNullOrEmpty(resource)) throw new ArgumentException(nameof(resource));

            try
            {
                var response = client.ReadKeyValueAsync(resource).Result;
                return response.Any();
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
                var sessionTtl = ttl.TotalSeconds < 10 ? 10 : ttl.TotalSeconds;
                var sessionResponse = client.CreateSessionAsync(new ConsulClient.CreateSessionRequest(GetSessionName(resource), "delete", (int)sessionTtl, options.LockDelay)).Result;
                if (sessionResponse.Success == false)
                    return false;

                var keyValueResponse = client.CreateKeyValueAsync(new ConsulClient.CreateKeyValueRequest(resource, null, sessionResponse.Id)).Result;
                return keyValueResponse;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Unable to acquire lock for resource '{resource}', ttl '{ttl.TotalSeconds}'");
                return false;
            }
        }

        public void Unlock(string resource)
        {
            try
            {
                client.DeleteKeyValueAsync(resource).Wait();
                client.DeleteSessionAsync(GetSessionName(resource)).Wait();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Unable to release lock for resource '{resource}'");
            }
        }

        private string GetSessionName(string resource) => $"session/{resource}";
    }
}

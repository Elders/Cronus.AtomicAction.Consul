using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cronus.AtomicAction.Consul
{
    public interface IConsulClient
    {
        Task<ConsulClient.CreateSessionResponse> CreateSessionAsync(ConsulClient.CreateSessionRequest request);

        ConsulClient.CreateSessionResponse CreateSession(string name, TimeSpan ttl, TimeSpan lockDelay);
        Task<IEnumerable<ConsulClient.ReadSessionResponse>> ReadSessionAsync(string id);
        Task<bool> DeleteSessionAsync(string id);

        Task<bool> CreateKeyValueAsync(ConsulClient.CreateKeyValueRequest request);
        Task<IEnumerable<ConsulClient.ReadKeyValueResponse>> ReadKeyValueAsync(string key, bool recurce = false);
        Task<bool> DeleteKeyValueAsync(string key);
    }
}

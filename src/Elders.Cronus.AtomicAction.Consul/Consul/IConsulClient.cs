using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cronus.AtomicAction.Consul
{
    public interface IConsulClient
    {
        Task<IEnumerable<ConsulClient.ReadKeyValueResponse>> ReadKeyValueAsync(string key, bool recurce = false);
        Task<bool> CreateKeyValueAsync(ConsulClient.CreateKeyValueRequest request);
        Task<ConsulClient.CreateSessionResponse> CreateSessionAsync(ConsulClient.CreateSessionRequest request);
        Task<bool> DeleteKeyValueAsync(string key);
        Task<bool> DeleteSessionAsync(string id);
    }
}

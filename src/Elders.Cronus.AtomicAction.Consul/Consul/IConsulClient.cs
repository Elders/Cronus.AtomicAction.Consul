using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cronus.AtomicAction.Consul
{
    public interface IConsulClient
    {
        Task<CreateSessionResponse> CreateSessionAsync(CreateSessionRequest request);
        Task<CreateSessionResponse> CreateSessionAsync(string name);
        Task<IEnumerable<ReadSessionResponse>> ReadSessionAsync(string id);
        Task<bool> DeleteSessionAsync(string id);
        Task<bool> CreateKeyValueAsync(CreateKeyValueRequest request);
        Task<IEnumerable<ReadKeyValueResponse>> ReadKeyValueAsync(string key, bool recurce = false);
        Task<bool> DeleteKeyValueAsync(string key);
        Task<bool> CreateKeyValueAsync(string revisionKey, int revision, string session);
    }
}

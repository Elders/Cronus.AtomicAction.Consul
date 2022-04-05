﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cronus.AtomicAction.Consul
{
    public interface IConsulClient
    {
        Task<CreateSessionResponse> CreateSessionAsync(CreateSessionRequest request);
        CreateSessionResponse CreateSession(string name);
        Task<IEnumerable<ReadSessionResponse>> ReadSessionAsync(string id);
        Task<bool> DeleteSessionAsync(string id);
        Task<bool> CreateKeyValueAsync(CreateKeyValueRequest request);
        Task<IEnumerable<ReadKeyValueResponse>> ReadKeyValueAsync(string key, bool recurce = false);
        Task<bool> DeleteKeyValueAsync(string key);
        bool CreateKeyValue(string revisionKey, int revision, string session);
    }
}
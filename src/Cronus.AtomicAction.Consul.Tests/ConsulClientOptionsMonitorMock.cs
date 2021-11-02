using System;
using Microsoft.Extensions.Options;

namespace Cronus.AtomicAction.Consul.Tests
{
    internal class ConsulClientOptionsMonitorMock : IOptionsMonitor<ConsulClientOptions>
    {
        public ConsulClientOptions CurrentValue => new ConsulClientOptions();

        public ConsulClientOptions Get(string name)
        {
            return CurrentValue;
        }

        public IDisposable OnChange(Action<ConsulClientOptions, string> listener)
        {
            return null;
        }
    }
}

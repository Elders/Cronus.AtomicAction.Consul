using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;

namespace Cronus.AtomicAction.Consul.Tests
{
    public class ConsulClientOptionsMonitorMock : IOptionsMonitor<ConsulClientOptions>
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

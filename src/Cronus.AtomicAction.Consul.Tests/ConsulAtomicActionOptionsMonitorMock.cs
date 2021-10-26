using System;
using Microsoft.Extensions.Options;

namespace Cronus.AtomicAction.Consul.Tests
{
    public class ConsulAtomicActionOptionsMonitorMock : IOptionsMonitor<ConsulAggregateRootAtomicActionOptions>
    {
        public ConsulAggregateRootAtomicActionOptions CurrentValue => new ConsulAggregateRootAtomicActionOptions();

        public IDisposable OnChange(Action<ConsulAggregateRootAtomicActionOptions, string> listener)
        {
            return null;
        }

        ConsulAggregateRootAtomicActionOptions IOptionsMonitor<ConsulAggregateRootAtomicActionOptions>.Get(string name)
        {
            return CurrentValue;
        }
    }
}

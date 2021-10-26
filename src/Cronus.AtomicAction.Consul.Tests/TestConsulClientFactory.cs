using Elders.Cronus.AtomicAction;

namespace Cronus.AtomicAction.Consul.Tests
{
    public static class TestConsulClientFactory
    {
        public static IConsulClient New()
        {
            return new ConsulClient(new ConsulClientOptionsMonitorMock());
        }
    }

}

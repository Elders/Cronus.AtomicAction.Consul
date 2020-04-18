using Elders.Cronus;
using Microsoft.Extensions.Configuration;

namespace Cronus.AtomicAction.Consul
{
    public class ConsulLockOptionsProvider : CronusOptionsProviderBase<ConsulLockOptions>
    {
        public ConsulLockOptionsProvider(IConfiguration configuration) : base(configuration) { }

        public override void Configure(ConsulLockOptions options)
        {
            configuration.GetSection("cronus:atomicaction:consul").Bind(options);
        }
    }
}

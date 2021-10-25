using Elders.Cronus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cronus.AtomicAction.Consul
{
    public class ConsulAggregateRootAtomicActionOptionsProvider : CronusOptionsProviderBase<ConsulAggregateRootAtomicActionOptions>
    {
        public ConsulAggregateRootAtomicActionOptionsProvider(IConfiguration configuration) : base(configuration) { }

        public override void Configure(ConsulAggregateRootAtomicActionOptions options)
        {
            configuration.GetSection("cronus:atomicaction:consul").Bind(options);
        }
    }
}

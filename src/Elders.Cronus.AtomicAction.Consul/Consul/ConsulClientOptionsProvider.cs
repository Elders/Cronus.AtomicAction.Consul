using Elders.Cronus;
using Microsoft.Extensions.Configuration;

namespace Cronus.AtomicAction.Consul
{
    public class ConsulClientOptionsProvider : CronusOptionsProviderBase<ConsulClientOptions>
    {
        public ConsulClientOptionsProvider(IConfiguration configuration) : base(configuration) { }

        public override void Configure(ConsulClientOptions options)
        {
            configuration.GetSection("cronus:atomicaction:consul").Bind(options);
        }
    }
}

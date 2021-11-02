using System;
using System.Collections.Generic;
using Elders.Cronus;
using Elders.Cronus.AtomicAction;
using Elders.Cronus.Discoveries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cronus.AtomicAction.Consul
{
    public class ConsulAggregateRootAtomicActionDiscovery : DiscoveryBase<IAggregateRootAtomicAction>
    {
        protected override DiscoveryResult<IAggregateRootAtomicAction> DiscoverFromAssemblies(DiscoveryContext context)
        {
            return new DiscoveryResult<IAggregateRootAtomicAction>(GetModels(), AddServices);
        }

        private void AddServices(IServiceCollection services)
        {
            services.AddOptions<ConsulClientOptions, ConsulClientOptionsProvider>();
            services.AddOptions<ConsulAggregateRootAtomicActionOptions, ConsulAggregateRootAtomicActionOptionsProvider>();

            services.AddHttpClient<IConsulClient, ConsulClient>("cronus.atomicaction", (provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<ConsulClientOptions>>().Value;
                var builder = new UriBuilder(options.Endpoint);

                client.BaseAddress = builder.Uri;
            });
        }

        IEnumerable<DiscoveredModel> GetModels()
        {
            yield return new DiscoveredModel(typeof(IAggregateRootAtomicAction), typeof(ConsulAggregateRootAtomicAction), ServiceLifetime.Singleton);
        }
    }
}

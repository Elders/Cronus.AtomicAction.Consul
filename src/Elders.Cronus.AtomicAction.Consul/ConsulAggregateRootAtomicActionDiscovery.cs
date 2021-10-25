using System.Collections.Generic;
using Elders.Cronus;
using Elders.Cronus.AtomicAction;
using Elders.Cronus.Discoveries;
using Microsoft.Extensions.DependencyInjection;

namespace Cronus.AtomicAction.Consul
{
    public class ConsulAggregateRootAtomicActionDiscovery : DiscoveryBase<IAggregateRootAtomicAction>
    {
        protected override DiscoveryResult<IAggregateRootAtomicAction> DiscoverFromAssemblies(DiscoveryContext context)
        {
            return new DiscoveryResult<IAggregateRootAtomicAction>(GetModels(context), services => services.AddOptions<ConsulClientOptions, ConsulClientOptionsProvider>()
                                                                                                           .AddOptions<ConsulAggregateRootAtomicActionOptions, ConsulAggregateRootAtomicActionOptionsProvider>());
        }

        IEnumerable<DiscoveredModel> GetModels(DiscoveryContext context)
        {
            yield return new DiscoveredModel(typeof(IAggregateRootAtomicAction), typeof(ConsulAggregateRootAtomicAction), ServiceLifetime.Transient);
            yield return new DiscoveredModel(typeof(IConsulClient), typeof(ConsulClient), ServiceLifetime.Singleton);
            yield return new DiscoveredModel(typeof(ILock), typeof(ConsulLock), ServiceLifetime.Singleton);
            yield return new DiscoveredModel(typeof(IRevisionStore), typeof(ConsulRevisionStore), ServiceLifetime.Singleton);
        }
    }
}

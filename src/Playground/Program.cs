using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cronus.AtomicAction.Consul;
using Elders.Cronus;
using Elders.Cronus.AtomicAction;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Playground
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "cronus:atomicaction:consul:endpoint", "http://consul.local.com:8500" },
                    { "cronus:atomicaction:consul:token", "authToken" },
                    { "cronus:atomicaction:consul:lockttl", "00:01:00.000" },
                    { "cronus:atomicaction:consul:revisionttl", "00:01:00.000" }
                }).Build();

            var servises = new ServiceCollection();
            servises.AddSingleton<IConfiguration>(configuration);
            servises.AddLogging(x => x.AddConsole());

            servises.Configure<ConsulClientOptions>(configuration);
            servises.Configure<ConsulAggregateRootAtomicActionOptions>(configuration);

            servises.AddOptions<ConsulClientOptions, ConsulClientOptionsProvider>();
            servises.AddOptions<ConsulAggregateRootAtomicActionOptions, ConsulAggregateRootAtomicActionOptionsProvider>();

            servises.AddSingleton<IConsulClient, ConsulClient>();
            servises.AddSingleton<ILock, ConsulLock>();
            servises.AddSingleton<IRevisionStore, ConsulRevisionStore>();
            servises.AddTransient<IAggregateRootAtomicAction, ConsulAggregateRootAtomicAction>();

            var serviceProvider = servises.BuildServiceProvider();
            var atomicAction = serviceProvider.GetRequiredService<IAggregateRootAtomicAction>();
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
            CronusLogger.SetStartupLogger(logger.CreateLogger<Program>());

            var id = new HeadquarterId("20ed0b20-0f7f-4659-9211-0bee5b693e51", "elders");

            for (int revision = 1; revision <= 5; revision++)
            {
                System.Threading.Thread.Sleep(100);
                var result = atomicAction.Execute(id, revision, () =>
                {
                    Console.WriteLine($"{DateTime.Now.TimeOfDay}-{id}");
                });

                Console.WriteLine($"{DateTime.Now.TimeOfDay}-{result.IsSuccessful}-{string.Join(", ", result.Errors)}");
            }

            Console.ReadKey();
        }
    }

    public class HeadquarterId : AggregateRootId
    {
        HeadquarterId() { }

        public HeadquarterId(AggregateRootId id) : base(id, "Headquarter", "elders") { }

        public HeadquarterId(string id, string tenant) : base(id, "Headquarter", tenant) { }
    }

    class TestId : AggregateRootId
    {
        public TestId() : base("e0846069-2730-4d3c-bc80-470d6a521d99", "testid", "elders")
        {
        }
    }
}

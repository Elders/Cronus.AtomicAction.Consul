using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Elders.Cronus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Playground.AtomTracker.Commands;

namespace Playground
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var hostBuilder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(x => x.AddEnvironmentVariables())
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions();
                    // services.AddLogging();
                    services.AddHostedService<Worker>();
                    services.AddCronus(hostContext.Configuration);
                });

            hostBuilder.Build().Run();
        }
    }

    public partial class Worker : BackgroundService
    {
        private readonly ICronusHost cronusHost;
        private readonly IPublisher<ICommand> publisher;

        public Worker(IServiceProvider provider, ICronusHost cronusHost, IPublisher<ICommand> publisher)
        {
            this.cronusHost = cronusHost;
            this.publisher = publisher;
            CronusBooter.BootstrapCronus(provider);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            Activity.DefaultIdFormat = ActivityIdFormat.W3C;
            Publish(publisher);
            cronusHost.Start();
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            cronusHost.Stop();

            return Task.CompletedTask;
        }

        public static void Publish(IPublisher<ICommand> publisher)
        {
            for (int i = 0; i < 10_000; i++)
            {
                var cmd = new BravoBeee(new AtomTracker.AtomTrackerId(i.ToString(), "elders"));
                publisher.Publish(cmd);
            }
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

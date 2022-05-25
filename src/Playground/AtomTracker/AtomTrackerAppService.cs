using Elders.Cronus;
using Playground.AtomTracker.Commands;
using System.Threading.Tasks;

namespace Playground.AtomTracker
{
    public class AtomTrackerAppService : ApplicationService<AtomTracker>,
        ICommandHandler<AtomEvent>
    {
        public AtomTrackerAppService(IAggregateRepository repo) : base(repo)
        {

        }

        public async Task HandleAsync(AtomEvent command)
        {
            AtomTracker ar = new AtomTracker(command.Id);
            await repository.SaveAsync(ar).ConfigureAwait(false);
        }
    }
}


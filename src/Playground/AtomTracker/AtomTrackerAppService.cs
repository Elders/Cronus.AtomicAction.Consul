using Elders.Cronus;
using Playground.AtomTracker.Commands;

namespace Playground.AtomTracker
{
    public class AtomTrackerAppService : ApplicationService<AtomTracker>,
        ICommandHandler<AtomEvent>
    {
        public AtomTrackerAppService(IAggregateRepository repo) : base(repo)
        {

        }

        public void Handle(AtomEvent command)
        {
            var ar = new AtomTracker(command.Id);
            repository.Save(ar);
        }
    }
}


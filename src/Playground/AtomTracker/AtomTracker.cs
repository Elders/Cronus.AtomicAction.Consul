using Elders.Cronus;

namespace Playground.AtomTracker
{
    public class AtomTracker : AggregateRoot<AtomTrackerState>
    {
        private AtomTracker() { }

        public AtomTracker(AtomTrackerId id)
        {
            Apply(new AggregateComittedEvent(id));
        }
    }
}

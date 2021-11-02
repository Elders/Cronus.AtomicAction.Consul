using Elders.Cronus;

namespace Playground.AtomTracker
{
    public class AtomTrackerState : AggregateRootState<AtomTracker, AtomTrackerId>
    {
        public override AtomTrackerId Id { get; set; }

        public int TotalRequests { get; set; }

        public void When(AggregateComittedEvent e)
        {
            Id = e.Id;
            TotalRequests++;
        }
    }
}

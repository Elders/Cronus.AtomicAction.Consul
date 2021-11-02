using System;
using System.Runtime.Serialization;
using Elders.Cronus;

namespace Playground.AtomTracker
{
    [DataContract(Namespace = "atom", Name = "c5b43828-b6da-47c1-961e-17ed433159ef")]
    public class AggregateComittedEvent : IEvent
    {
        private AggregateComittedEvent() { }

        public AggregateComittedEvent(AtomTrackerId id)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        [DataMember(Order = 1)]
        public AtomTrackerId Id { get; private set; }
    }
}

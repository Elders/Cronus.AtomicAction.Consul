using System.Runtime.Serialization;
using Elders.Cronus;
namespace Playground.AtomTracker
{
    [DataContract(Namespace = "atom", Name = "792ebf74-37aa-4adc-8204-9a7a72e7e54f")]
    public class AtomTrackerId : AggregateRootId<AtomTrackerId>
    {
        private AtomTrackerId() { }

        public AtomTrackerId(string id, string tenant) : base(id, "atomtracker", tenant) { }

        public AtomTrackerId(IUrn userUrn, string tenant) : base(userUrn.NSS, "atomtracker", tenant) { }

        protected override AtomTrackerId Construct(string id, string tenant) => new AtomTrackerId(id, tenant);
    }
}

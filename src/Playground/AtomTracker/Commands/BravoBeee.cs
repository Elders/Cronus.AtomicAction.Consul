using System.Runtime.Serialization;
using System.Windows.Input;
using Elders.Cronus;

namespace Playground.AtomTracker.Commands
{
    [DataContract(Name = "e5b5519f-1060-4a90-8156-0d34c4c9f231")]
    public class BravoBeee : Elders.Cronus.ICommand
    {
        private BravoBeee() { }

        public BravoBeee(AtomTrackerId id)
        {
            Id = id;
        }

        [DataMember(Order = 1)]
        public AtomTrackerId Id { get; private set; }
    }
}

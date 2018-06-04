using Common;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ActorSwarm.Interfaces
{
    [DataContract]
    [KnownType(typeof(ResidentState))]
    [KnownType(typeof(Resident))]
    public sealed class SwarmState
    {
        [DataMember]
        public Shared2DArray<byte> SharedState { get; set; }
        [DataMember]
        public List<ResidentState> VirtualActorStates { get; set; }
        [DataMember]
        public List<IVirtualActor> VirtualActors { get; set; }
    }
}
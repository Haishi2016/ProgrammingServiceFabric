using System;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class Proposal2D<T> : IProposal
        where T : IComparable
    {
        [DataMember]
        public int ActorId { get; set; }
        [DataMember]
        public int OldX { get; set; }
        [DataMember]
        public int OldY { get; set; }
        [DataMember]
        public int NewX { get; set; }
        [DataMember]
        public int NewY { get; set; }
        [DataMember]
        public T ProposedValue { get; set; }
        public Proposal2D(int id, int oldX, int oldY, int newX, int newY, T proposal)
        {
            ActorId = id;
            OldX = oldX;
            OldY = oldY;
            NewX = newX;
            NewY = newY;
            ProposedValue = proposal;
        }
    }
}

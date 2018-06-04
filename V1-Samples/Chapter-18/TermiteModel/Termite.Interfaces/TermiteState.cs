using System.Runtime.Serialization;

namespace Termite.Interfaces
{
    [DataContract]
    public class TermiteState
    {
        [DataMember]
        public int X { get; set; }
        [DataMember]
        public int Y { get; set; }
        [DataMember]
        public bool HasWoodChip { get; set; }
    }
}

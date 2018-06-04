using System;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract(Name = "ResidentState")]
    public class ResidentState : IComparable
    {
        [DataMember]
        public int X { get; set; }
        [DataMember]
        public int Y { get; set; }
        [DataMember]
        public byte Tag { get; set; }

        public int CompareTo(object obj)
        {
            if (obj != null && obj is ResidentState)
            {
                ResidentState that = (ResidentState)obj;
                if (that.X == this.X && that.Y == this.Y && that.Tag == this.Tag)
                    return 0;
            }
            return -1;
        }
    }
}

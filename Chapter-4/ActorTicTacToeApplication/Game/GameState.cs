using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    [DataContract]
    public class GameState
    {
        [DataMember]
        public int[] Board;
        [DataMember]
        public string Winner;
        [DataMember]
        public List<Tuple<long, string>> Players;
        [DataMember]
        public int NextPlayerIndex;
        [DataMember]
        public int NumberOfMoves;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    [DataContract]
    public class GameActorState
    {
        /*
         * The game board (the Board property) is presented as a nine-member byte array. Each item in
         * the array can be 0 (empty), -1 (player 1 piece), or 1 (player 2 piece).
         */
        [DataMember]
        public int[] Board;
        /* 
         * Holds the name of the game winner. When the game is in progress, the value is empty. When a player
         * wins, this property is set to the name of the winner.
         */
        [DataMember]
        public string Winner;
        /*
         * Holds the list of players.
         */
        [DataMember]
        public List<Tuple<long, string>> Players;
        /*
         * Indicates which player has the next turn. 
         */
        [DataMember]
        public int NextPlayerIndex;
        /*
         * Tracks how many pieces have been put on the board. 
         */
        [DataMember]
        public int NumberOfMoves;
    }
}

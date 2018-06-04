using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using Game.Interfaces;

namespace Game
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    public class Game : Actor, IGame
    {
        /// <summary>
        /// Initializes a new instance of Game
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public Game(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {

        }

        public Task<int[]> GetGameBoardAsync()
        {
            var state = this.StateManager.TryGetStateAsync<GameActorState>("GameActorState");
            if (state.Result.HasValue)
            {
                var gState = state.Result.Value;
                return Task.FromResult<int[]>(gState.Board);
            }
            return Task.FromResult<int[]>(null);
        }

        public Task<string> GetWinnerAsync()
        {
            var state = this.StateManager.TryGetStateAsync<GameActorState>("GameActorState");
            if (state.Result.HasValue)
            {
                var gState = state.Result.Value;
                return Task.FromResult<string>(gState.Winner);
            }
            return Task.FromResult<string>(null);
        }

        public Task<bool> JoinGameAsync(long playerId, string playerName)
        {
            var state = this.StateManager.TryGetStateAsync<GameActorState>("GameActorState");
            if (state.Result.HasValue)
            {
                var gState = state.Result.Value;
                if (gState.Players.Count >= 2
                    || gState.Players.FirstOrDefault(p => p.Item2 == playerName) != null)
                {
                    return Task.FromResult<bool>(false);
                }
                gState.Players.Add(new Tuple<long, string>(playerId, playerName));
                this.StateManager.SetStateAsync("GameActorState", gState);

                return Task.FromResult<bool>(true);
            }
            return Task.FromResult<bool>(false);
        }

        public Task<bool> MakeMoveAsync(long playerId, int x, int y)
        {
            var state = this.StateManager.TryGetStateAsync<GameActorState>("GameActorState");
            if (state.Result.HasValue == false)
            {
                return Task.FromResult<bool>(false);
            }

            var gState = state.Result.Value;
            if (x < 0 || x > 2 || y < 0 || y > 2
                || gState.Players.Count != 2
                || gState.NumberOfMoves >= 9
                || gState.Winner != "")
            {
                return Task.FromResult<bool>(false);
            }

            int index = gState.Players.FindIndex(p => p.Item1 == playerId);
            if (index != gState.NextPlayerIndex)
            {
                return Task.FromResult<bool>(false);
            }

            if (gState.Board[y * 3 + x] != 0)
            {
                // The cell is not empty.
                return Task.FromResult<bool>(false);
            }
            int piece = index * 2 - 1;
            gState.Board[y * 3 + x] = piece;
            gState.NumberOfMoves++;

            if (HasWon(gState, piece * 3))
            {
                gState.Winner =
                    gState.Players[index].Item2
                    + " (" + (piece == -1 ? "X" : "O") + ")";
            }
            else if (gState.Winner == "" && gState.NumberOfMoves >= 9)
            {
                gState.Winner = "TIE";
            }

            gState.NextPlayerIndex = (gState.NextPlayerIndex + 1) % 2;

            return Task.FromResult<bool>(true);
        }

        private bool HasWon(object state, int sum)
        {
            var gState = state as GameActorState;
            return gState.Board[0] + gState.Board[1] + gState.Board[2] == sum
                || gState.Board[3] + gState.Board[4] + gState.Board[5] == sum
                || gState.Board[6] + gState.Board[7] + gState.Board[8] == sum
                || gState.Board[0] + gState.Board[3] + gState.Board[6] == sum
                || gState.Board[1] + gState.Board[4] + gState.Board[7] == sum
                || gState.Board[2] + gState.Board[5] + gState.Board[8] == sum
                || gState.Board[0] + gState.Board[4] + gState.Board[8] == sum
                || gState.Board[2] + gState.Board[4] + gState.Board[6] == sum;
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, $"Actor {this.GetType().Name} activating...");

            var state = this.StateManager.TryGetStateAsync<GameActorState>("GameActorState");
            if (state.Result.HasValue == false)
            {
                this.StateManager.SetStateAsync("GameActorState", new GameActorState()
                {
                    Board = new int[9],
                    Winner = "",
                    Players = new List<Tuple<long, string>>(),
                    NextPlayerIndex = 0,
                    NumberOfMoves = 0
                });
            }

            ActorEventSource.Current.ActorMessage(this, $"Actor {this.GetType().Name} activated...");

            return this.StateManager.GetStateAsync<GameActorState>("GameActorState");
        }
    }
}

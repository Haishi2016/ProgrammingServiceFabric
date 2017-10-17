using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using Player.Interfaces;
using Game.Interfaces;

namespace Player
{
   
    [StatePersistence(StatePersistence.None)]
    internal class Player : Actor, IPlayer
    {
        public Player(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        public Task<bool> JoinGameAsync(ActorId gameId, string playerName)
        {
            var game = ActorProxy.Create<IGame>(gameId, "fabric:/ActorTicTacToeApplication");
            return game.AcceptPlayerToGameAsync(this.Id.GetLongId(), playerName);
        }

        public Task<bool> MakeMoveAsync(ActorId gameId, int x, int y)
        {
            var game = ActorProxy.Create<IGame>(gameId, "fabric:/ActorTicTacToeApplication");
            return game.AcceptPlayerMoveAsync(this.Id.GetLongId(), x, y);
        }

        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");
            return Task.FromResult<bool>(true);
        }

    }
}

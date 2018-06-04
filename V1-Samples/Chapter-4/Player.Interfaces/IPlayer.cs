using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace Player.Interfaces
{
    /// <summary>
    /// This interface defines the methods exposed by an actor.
    /// Clients use this interface to interact with the actor that implements it.
    /// </summary>
    public interface IPlayer : IActor
    {
        Task<bool> JoinGameAsync(ActorId gameId, string playerName);
        Task<bool> MakeMoveAsync(ActorId gameId, int x, int y);
    }
}

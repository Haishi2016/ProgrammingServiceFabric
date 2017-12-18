using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace Game.Interfaces
{
    public interface IGame : IActor
    {
        Task<bool> AcceptPlayerToGameAsync(long playerId, string playerName);
        Task<int[]> GetGameBoardAsync();
        Task<string> GetWinnerAsync();
        Task<bool> AcceptPlayerMoveAsync(long playerId, int x, int y);
    }
}

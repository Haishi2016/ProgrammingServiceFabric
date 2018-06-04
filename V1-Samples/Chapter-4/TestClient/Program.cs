using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Game.Interfaces;
using Player.Interfaces;

namespace TestClient
{
    public class Program
    {
        [MTAThread]
        static void Main(string[] args)
        {
            var player1 = ActorProxy.Create<IPlayer>(ActorId.CreateRandom(), "fabric:/ActorTicTacToeApplication");
            var player2 = ActorProxy.Create<IPlayer>(ActorId.CreateRandom(), "fabric:/ActorTicTacToeApplication");

            var gameId = ActorId.CreateRandom();
            var game = ActorProxy.Create<IGame>(gameId, "fabric:/ActorTicTacToeApplication");
            var rand = new Random();

            var result1 = player1.JoinGameAsync(gameId, "Player 1");
            var result2 = player2.JoinGameAsync(gameId, "Player 2");
            if (!result1.Result || !result2.Result)
            {
                Console.WriteLine("Failed to join game.");
                return;
            }

            var player1Task = Task.Run(() => { MakeMove(player1, game, gameId); });
            var player2Task = Task.Run(() => { MakeMove(player2, game, gameId); });
            var gameTask = Task.Run(() =>
            {
                string winner = "";
                while (winner == "")
                {
                    var board = game.GetGameBoardAsync().Result;
                    PrintBoard(board);
                    winner = game.GetWinnerAsync().Result;
                    Task.Delay(2000).Wait();
                }
                Console.WriteLine("Winner is: " + winner);
            });
            gameTask.Wait();
            Console.Read();
        }

        private static async void MakeMove(IPlayer player, IGame game, ActorId gameId)
        {
            Random rand = new Random();
            while (true)
            {
                await player.MakeMoveAsync(gameId, rand.Next(0, 3), rand.Next(0, 3));
                await Task.Delay(rand.Next(500, 2000));
            }
        }

        private static void PrintBoard(int[] board)
        {
            Console.Clear();
            for (int i = 0; i < board.Length; i++)
            {
                if (board[i] == -1)
                    Console.Write(" X ");
                else if (board[i] == 1)
                    Console.Write(" O ");
                else
                    Console.Write(" . ");
                if ((i + 1) % 3 == 0)
                    Console.WriteLine();
            }
        }
    }
}
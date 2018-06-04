using Box.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Actors.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Termite.Interfaces;
using Microsoft.ServiceFabric.Actors;
using System.Threading;

namespace ModelTest
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Stateful Reliable Service PartitionKey generator.
            Random pKey = new Random(Guid.NewGuid().GetHashCode());

            int size = 100;
            int termites = 75;

            IBox boxClient = ServiceProxy.Create<IBox>(new Uri("fabric:/TermiteModel/Box"),
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(pKey.Next()));

            boxClient.ResetBox().Wait();

            ITermite[] proxies = new ITermite[termites];
            for (int i = 0; i < proxies.Length; i++)
            {
                proxies[i] = ActorProxy.Create<ITermite>(new ActorId(i), new Uri("fabric:/TermiteModel/TermiteActorService"));
            }

            while (true)
            {
                var box = boxClient.ReadBox().Result;
                Console.ForegroundColor = ConsoleColor.Cyan;

                for (int y = 0; y < size; y++)
                {
                    Console.CursorTop = y;
                    for (int x = 0; x < size; x++)
                    {
                        Console.CursorLeft = x;
                        if (box[y * size + x] == 0)
                            Console.Write(" ");
                        else
                            Console.Write("#");
                    }
                }

                Console.ForegroundColor = ConsoleColor.DarkRed;
                for (int i = 0; i < proxies.Length; i++)
                {
                    var state = proxies[i].GetStateAsync().Result;
                    Console.CursorLeft = state.X;
                    Console.CursorTop = state.Y;
                    Console.Write("T");
                }
                Thread.Sleep(500);
            }
        }
    }
}

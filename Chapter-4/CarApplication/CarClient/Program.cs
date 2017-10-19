using CarActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CarClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press any key to activate a random Car actor");
            Console.WriteLine("The exact contract doesn't matter - we are just activating the timer in this case.");
            Console.ReadLine();
            var proxy = ActorProxy.Create<ICarActor>(ActorId.CreateRandom(), new Uri("fabric:/CarApplication/CarActorService"));
            proxy.GetLocationAsync(new CancellationToken()).Wait();
        }
    }
}

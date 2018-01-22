using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Client;
using WordCounter.Interfaces;
using Microsoft.ServiceFabric.Actors;
using System.Threading;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ReadLine();
            var proxy = ActorProxy.Create<IWordCounter>(new ActorId("1"), new Uri("fabric:/AudioTranscriptionApp/WordCounterActorService"));
            proxy.CountWordsAsync("This is a test of some words in a common test.", default(CancellationToken)).Wait();
        }
    }
}

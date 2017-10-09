using Grpc.Core;
using Microsoft.ServiceFabric.Services.Client;
using System;
using System.Linq;

namespace gRPCClient
{
    class Program
    {
        static Random mRand = new Random();
        static void Main(string[] args)
        {
            ServicePartitionResolver partitionResolver = new ServicePartitionResolver("localhost:19000");
            var partition = partitionResolver.ResolveAsync(new Uri("fabric:/gRPCApplication/gRPCServer"),
                ServicePartitionKey.Singleton, new System.Threading.CancellationToken()).Result;
            var endpoint = partition.Endpoints.ElementAt(mRand.Next(0, partition.Endpoints.Count));

            var address = endpoint.Address.Substring(endpoint.Address.IndexOf("\"\":\"") + 4);
            address = address.Substring(0, address.IndexOf("\""));

            Channel channel = new Channel(address, ChannelCredentials.Insecure);
            var client = new Calculator.Calculator.CalculatorClient(channel);
            var reply = client.Add(new Calculator.CalculateRequest { A = 100, B = 200 });
            Console.WriteLine(string.Format("Replica {0} returned: {1} ", reply.ReplicaId, reply.Result));
        }
    }
}

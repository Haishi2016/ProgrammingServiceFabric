using CalculatorService.Interfaces;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CalculatorClient
{
    class Program
    {
        static void Main(string[] args)
        {
            
            var proxyFactory = new ServiceProxyFactory((c) =>
            {
                return new FabricTransportServiceRemotingClientFactory(
                    serializationProvider: new ServiceRemotingJsonSerializationProvider(),
                    servicePartitionResolver: new ServicePartitionResolver(
                        "localhost:19000",
                        "localhost:19001"
                        )
                    );
            });

            var calculatorClient = proxyFactory.CreateServiceProxy<ICalculatorService>(
                new Uri("fabric:/CalculatorApplication/CalculatorService"));
            var result = calculatorClient.Add(1, 2).Result;
            Console.WriteLine(result);
            Console.ReadKey();
        }
    }
}

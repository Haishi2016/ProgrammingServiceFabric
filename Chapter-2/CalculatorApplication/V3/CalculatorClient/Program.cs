using CalculatorService.Interfaces;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

namespace CalculatorClient 
{
    class Program
    {
        static void Main(string[] args)
        {
            
            IServicePartitionResolver partitionResolver = new ServicePartitionResolver("localhost:19000");
            var binding = WcfUtility.CreateTcpClientBinding();

         
              var wcfClientFactory = new WcfCommunicationClientFactory<ICalculatorService>
                (clientBinding: binding, servicePartitionResolver: partitionResolver);        
            
            for (int i = 0; i < 10; i++)
            {
                var calculatorServiceCommunicationClient = new ServicePartitionClient<WcfCommunicationClient<ICalculatorService>>(
                    wcfClientFactory,
                    new Uri("fabric:/CalculatorApplication/CalculatorService"));
                
                var result = calculatorServiceCommunicationClient.InvokeWithRetryAsync(
                    client => client.Channel.Add(2, 3)).Result;
                Console.WriteLine(result);
            }
        }
    }   
}

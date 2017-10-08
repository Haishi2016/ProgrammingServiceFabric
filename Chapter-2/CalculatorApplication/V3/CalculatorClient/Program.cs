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
            BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.None);

         
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
    class SampleServiceClient : System.ServiceModel.ClientBase<ICalculatorService>, ICalculatorService
    {
        public SampleServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress)
       :
           base(binding, remoteAddress)
        {
        }

        public Task<string> Add(int a, int b)
        {
            return base.Channel.Add(a,b);
        }

        public Task<string> Subtract(int a, int b)
        {
            return base.Channel.Subtract(a, b);
        }
    }
}

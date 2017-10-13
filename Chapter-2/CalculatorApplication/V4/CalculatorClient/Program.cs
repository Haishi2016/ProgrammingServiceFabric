using CalculatorService.Interfaces;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            
            IServicePartitionResolver partitionResolver = new ServicePartitionResolver("sfv2.eastus.cloudapp.azure.com:19000");
            BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.None);


            var resolveResults = partitionResolver.ResolveAsync(new Uri("fabric:/CalculatorApplication/CalculatorService"),
                    ServicePartitionKey.Singleton,
                    TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), new System.Threading.CancellationToken()).Result;

            var endpoint = resolveResults.GetEndpoint();
            var endpointObject = JsonConvert.DeserializeObject<JObject>(endpoint.Address);
            var addressString = ((JObject)endpointObject.Property("Endpoints").Value)[""].Value<string>();
            var endpointAddress = new EndpointAddress(addressString);
            var channel = ChannelFactory<ICalculatorService>.CreateChannel(binding, endpointAddress);
  
            for (int i = 0; i < 10; i++)
            {
                var calculatorServiceCommunicationClient = new WcfCommunicationClient<ICalculatorService>(channel);
                var result = calculatorServiceCommunicationClient.Channel.Add(2, 3).Result;
                Console.WriteLine(result);
            }
        }
    }
    
}

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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CalculatorClient 
{
    class Program
    {
        static void Main(string[] args)
        {
            string hostName = "sfv2.eastus.cloudapp.azure.com";

            Regex ipRex = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
            IServicePartitionResolver partitionResolver = new ServicePartitionResolver(hostName + ":19000");
            var binding = WcfUtility.CreateTcpClientBinding();
            

            var resolveResults = partitionResolver.ResolveAsync(new Uri("fabric:/CalculatorApplication/CalculatorService"),
                    ServicePartitionKey.Singleton,
                    TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10), new System.Threading.CancellationToken()).Result;

            var endpoint = resolveResults.GetEndpoint();
            var endpointObject = JsonConvert.DeserializeObject<JObject>(endpoint.Address);
            var addressString = ((JObject)endpointObject.Property("Endpoints").Value)[""].Value<string>();
            addressString = ipRex.Replace(addressString, hostName, 1);
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

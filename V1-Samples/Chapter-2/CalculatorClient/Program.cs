using CalculatorService;
using CalculatorServiceWCF;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;
using System.Fabric;
using System.ServiceModel;

namespace CalculatorClient
{
    public class Program
    {
        private static void Main(string[] args)
        {
            while (true)
            {
                /* Call #1: Stateless Service */
                var calculatorClient = ServiceProxy.Create<ICalculatorService>(new
                    Uri("fabric:/CalculatorApplication/CalculatorService"));
                var result = calculatorClient.Add(1, 2).Result;

                Console.WriteLine(result);
                System.Threading.Thread.Sleep(3000);


                /* Call #2: WCF Stateless Service */
                Uri serviceName = new Uri("fabric:/CalculatorApplication/CalculatorServiceWCF");
                ServicePartitionResolver serviceResolver = new ServicePartitionResolver(() =>
                    new FabricClient());

                NetTcpBinding binding = CreateClientConnectionBinding();
                WCFClient calcClient = new WCFClient(new WcfCommunicationClientFactory<ICalculatorServiceWCF>
                    (binding, null), serviceName);
                Console.WriteLine(calcClient.Add(3, 5).Result);
                Console.ReadKey();

            }
        }

        private static NetTcpBinding CreateClientConnectionBinding()
        {
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None)
            {
                SendTimeout = TimeSpan.MaxValue,
                ReceiveTimeout = TimeSpan.MaxValue,
                OpenTimeout = TimeSpan.FromSeconds(5),
                CloseTimeout = TimeSpan.FromSeconds(5),
                MaxReceivedMessageSize = 1024 * 1024
            };
            binding.MaxBufferSize = (int)binding.MaxReceivedMessageSize;
            binding.MaxBufferPoolSize = Environment.ProcessorCount * binding.
            MaxReceivedMessageSize;
            return binding;
        }
    }
}

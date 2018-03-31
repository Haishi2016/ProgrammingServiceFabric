using Common;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStoreClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Uri serviceName = new Uri("fabric:/SimpleStoreApplication/ShoppingCartService");
            ServicePartitionResolver serviceResolver = new ServicePartitionResolver(
                () => new FabricClient());

            NetTcpBinding binding = CreateClientConnectionBinding();

            // Call the service with different customer ids;
            for (int i = 0; i < 10; i++)
            {
                Client shoppingClient = new Client(new
                    WcfCommunicationClientFactory<IShoppingCartService>(binding, null, serviceResolver, null), serviceName, i);

                // Populate cart.
                shoppingClient.AddItem(new ShoppingCartItem
                {
                    ProductName = "XBOX ONE",
                    UnitPrice = 329.0,
                    Amount = 2
                }).Wait();
                shoppingClient.AddItem(new ShoppingCartItem
                {
                    ProductName = "Halo 5 (" + i.ToString() + ")",
                    UnitPrice = 59.99,
                    Amount = 1
                }).Wait();

                PrintPartition(shoppingClient);

                var list = shoppingClient.GetItems().Result;

                foreach (var item in list)
                {
                    Console.WriteLine(string.Format("{0}: {1:C2} X {2} = {3:C2}",
                    item.ProductName,
                    item.UnitPrice,
                    item.Amount,
                    item.LineTotal));
                }
            }
            Console.ReadKey();
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

            binding.MaxBufferSize = (int) binding.MaxReceivedMessageSize;
            binding.MaxBufferPoolSize = Environment.ProcessorCount * binding.
            MaxReceivedMessageSize;
            return binding;
        }

        private static void PrintPartition(Client client)
        {
            ResolvedServicePartition partition;
            if (client.TryGetLastResolvedServicePartition(out partition))
            {
                Console.WriteLine("Partition ID: " + partition.Info.Id);
            }
        }
    }
}

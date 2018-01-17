using Common;
using GlobalActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] products = { "VCR", "Fax", "CassettePlayer", "Camcorder", "GameConsole",
                            "CD", "TV", "Radio", "Phone", "Karaoke"};
            string[] countries = { "US", "China", "Australia" };

            DateTime startTime = DateTime.Now;

            for (int i = 0; i < 10; i++)
            {
                Thread t = new Thread(() =>
                {
                    Random rand = new Random();
                    while (true)
                    {
                        SimulateSales(countries[rand.Next(0, countries.Length)], products[rand.Next(0, products.Length)]);
                        Thread.Sleep(100);
                    }
                });
                t.Start();
            }
            while (true)
            {
                Console.CursorLeft = 0;
                Console.CursorTop = 0;
                var nation = ActorProxy.Create<IGlobalActor>(new ActorId("1"), "fabric:/ECommerceApp");
                var list = nation.CountGlobalSalesAsync().Result;
                long count = 0;
                Console.WriteLine();
                foreach (var result in list)
                {
                    Console.WriteLine(result.Item1.PadLeft(18, ' ') + ": " + result.Item2.ToString().PadRight(10, ' '));
                    count += result.Item2;
                }
                Console.WriteLine(string.Format("\nTPS: {0:0.00}", count / (DateTime.Now - startTime).TotalSeconds));
                Thread.Sleep(100);
            }
        }
        static void SimulateSales(string country, string product)
        {
            using (GatewayWebSocketClient websocketClient = new GatewayWebSocketClient())
            {
                websocketClient.ConnectAsync("ws://localhost:31024/SalesServiceWS/").Wait();
                PostSalesModel postSalesModel = new PostSalesModel
                {
                    Product = product,
                    Country = country
                };

                IWsSerializer serializer = SerializerFactory.CreateSerializer();
                byte[] payload = serializer.SerializeAsync(postSalesModel).Result;

                WsRequestMessage mreq = new WsRequestMessage
                {
                    Operation = "sell",
                    Value = payload
                };

                WsResponseMessage mresp = websocketClient.SendReceiveAsync(mreq, CancellationToken.None).Result;
                if (mresp.Result == WsResult.Error)
                    Console.WriteLine("Error: {0}", Encoding.UTF8.GetString(mresp.Value));
            }
        }
    }
}

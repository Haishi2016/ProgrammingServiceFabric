using Microsoft.Azure.EventHubs;
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
        static Random random = new Random();
        static void Main(string[] args)
        {
            ThreadPool.QueueUserWorkItem((a) => SendMessages());
            ThreadPool.QueueUserWorkItem((a) => ReceiveMessages("0"));
            ThreadPool.QueueUserWorkItem((a) => ReceiveMessages("1"));    
            Console.ReadKey();
        }
        static void SendMessages()
        {
            var connectionStringBuilder = new EventHubsConnectionStringBuilder("[Event Hub Connection String]")
            {
                EntityPath = "number-ingress"
            };
            var eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
            while (true)
            {
                try
                {
                    var message = random.Next(0, int.MaxValue);
                    Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, message);
                    eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(message.ToString()))).Wait();
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0} > Exception: {1}", DateTime.Now, exception.Message);
                    Console.ResetColor();
                }

                Thread.Sleep(200);
            }
        }
        static void ReceiveMessages(string partitionId)
        {
            var connectionStringBuilder = new EventHubsConnectionStringBuilder("[Event Hub Connection String]")
            {
                EntityPath = "number-egress"
            };
            var eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
            var receiver = eventHubClient.CreateEpochReceiver("$Default", partitionId, DateTime.Now, 1);
            
            while (true)
            {
                var data = receiver.ReceiveAsync(5).Result;
                if (data != null)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    foreach (var item in data) {
                        Console.WriteLine("{0} - {2} > Data: {1}", DateTime.Now, Encoding.UTF8.GetString(item.Body.Array), partitionId);
                    }
                    Console.ResetColor();
                }
            }
        }
    }
}

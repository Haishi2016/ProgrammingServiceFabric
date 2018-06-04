using CalculatorService;
using CalculatorServiceWCF;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.Threading.Tasks;


namespace CalculatorClient
{
    public class WCFClient : ServicePartitionClient<WcfCommunicationClient<ICalculatorServiceWCF>>,
        ICalculatorService
    {
        public WCFClient(WcfCommunicationClientFactory<ICalculatorServiceWCF> clientFactory, Uri serviceName)
            : base(clientFactory, serviceName)
        {
        }

        public Task<string> Add(int a, int b)
        {
            return this.InvokeWithRetryAsync(client => client.Channel.Add(a, b));
        }

        public Task<string> Subtract(int a, int b)
        {
            return this.InvokeWithRetryAsync(client => client.Channel.Subtract(a, b));
        }
    }
}

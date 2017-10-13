using CalculatorService.Interfaces;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Microsoft.ServiceFabric.Services.Client;
using System.Fabric;

namespace CalculatorClient
{
    public class WcfCommunicationClient<TServiceContract> : ICommunicationClient
        where TServiceContract : class
    {
        private readonly TServiceContract channel;
        internal WcfCommunicationClient(TServiceContract channel)

        {
            this.channel = channel;
        }
        public ResolvedServicePartition ResolvedServicePartition { get; set; }
        public string ListenerName { get; set; }
        public ResolvedServiceEndpoint Endpoint { get; set; }
        public TServiceContract Channel

        {
            get { return this.channel; }
        }
        
        internal IClientChannel ClientChannel
        {
            get { return (IClientChannel)this.Channel; }
        }

        ~WcfCommunicationClient()
        {
            this.ClientChannel.Abort();
        }
    }
}

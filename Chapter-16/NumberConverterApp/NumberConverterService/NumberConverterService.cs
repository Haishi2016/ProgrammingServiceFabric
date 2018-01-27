using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;

namespace NumberConverterService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class NumberConverterService : StatelessService
    {
        public NumberConverterService(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            const string EhConnectionString = "Endpoint=sb://number-converter.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=qN6Kac7d7dJ7H+mTzjxEKR4LiWENzY90Yc64ZnFzPXA=";
            const string EhEntityPath = "number-ingress";
            const string StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=numberconverter;AccountKey=baKsxPdPwdiZfBLNdUm5BMAdayqW87DyyHJ6It5T5vaWKJdW2leWqD4AIv0ks3Ox0ZqrJkrNFdlkL2lYnkufyQ==;EndpointSuffix=core.windows.net";
            const string StorageContainerName = "numbers";

            var eventProcessorHost = new EventProcessorHost(
            EhEntityPath,
            PartitionReceiver.DefaultConsumerGroupName,
            EhConnectionString,
            StorageConnectionString,
            StorageContainerName);

            // Registers the Event Processor Host and starts receiving messages
            await eventProcessorHost.RegisterEventProcessorAsync<NumberConverter>();
        }
    }
}

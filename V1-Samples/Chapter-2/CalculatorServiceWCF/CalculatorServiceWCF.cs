using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace CalculatorServiceWCF
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class CalculatorServiceWCF : StatelessService, ICalculatorServiceWCF
    {
        public CalculatorServiceWCF(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]
            {
                new ServiceInstanceListener((context) =>
                    new WcfCommunicationListener<ICalculatorServiceWCF>(
                        serviceContext: context,
                        wcfServiceObject: this,
                        endpointResourceName: "ServiceEndpoint",
                        listenerBinding: this.CreateListenBinding()
                        ))
            };
        }

        private NetTcpBinding CreateListenBinding()
        {
            NetTcpBinding binding = new NetTcpBinding(securityMode: SecurityMode.None)
            {
                SendTimeout = TimeSpan.MaxValue,
                ReceiveTimeout = TimeSpan.MaxValue,
                OpenTimeout = TimeSpan.FromSeconds(5),
                CloseTimeout = TimeSpan.FromSeconds(5),
                MaxConnections = int.MaxValue,
                MaxReceivedMessageSize = 1024 * 1024
            };

            binding.MaxBufferSize = (int)binding.MaxReceivedMessageSize;
            binding.MaxBufferPoolSize
                = Environment.ProcessorCount * binding.MaxReceivedMessageSize;

            return binding;
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        public Task<string> Add(int a, int b)
        {
            return Task.FromResult<string>(
                string.Format("Instance {0} returns: {1}", this.Context.InstanceId, a + b)
                );
        }

        public Task<string> Subtract(int a, int b)
        {
            return Task.FromResult<string>(
                string.Format("Instance {0} returns: {1}", this.Context.InstanceId, a - b));
        }
    }
}

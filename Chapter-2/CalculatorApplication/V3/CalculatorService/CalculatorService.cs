using CalculatorService.Interfaces;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;

namespace CalculatorService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class CalculatorService : StatelessService, ICalculatorService
    {
        public CalculatorService(StatelessServiceContext context)
            : base(context)
        { }
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]
            {
                new ServiceInstanceListener((context) =>
                    new WcfCommunicationListener<ICalculatorService>(
                        wcfServiceObject: this,
                        serviceContext: context,
                        endpointResourceName: "ServiceEndpoint",
                        listenerBinding: WcfUtility.CreateTcpListenerBinding()
                   ))
            };
        }
        public Task<string> Add(int a, int b)
        {
            return Task.FromResult<string>(string.Format("Instance {0} returns: {1}",
                this.Context.InstanceId,
                a + b));
        }
        public Task<string> Subtract(int a, int b)
        {
            return Task.FromResult<string>(string.Format("Instance {0} returns: {1}",
               this.Context.InstanceId,
               a - b));
        }
    }
}

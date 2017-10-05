using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using CalculatorService.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;

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
                new ServiceInstanceListener((c) =>
                    {
                        return new FabricTransportServiceRemotingListener(c, this, null,
                        new ServiceRemotingJsonSerializationProvider());
                    })
            };

        }
        public Task<long> Add(int a, int b)
        {
            return Task.FromResult<long>(a + b);
        }
        public Task<long> Subtract(int a, int b)
        {
            return Task.FromResult<long>(a - b);
        }
    }
}

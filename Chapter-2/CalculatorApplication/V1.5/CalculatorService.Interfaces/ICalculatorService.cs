using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: FabricTransportServiceRemotingProvider(RemotingListener = RemotingListener.V2Listener, RemotingClient = RemotingClient.V2Client)]
namespace CalculatorService.Interfaces
{
    public interface ICalculatorService: IService
    {
        Task<long> Add(int a, int b);
        Task<long> Subtract(int a, int b);
    }
}

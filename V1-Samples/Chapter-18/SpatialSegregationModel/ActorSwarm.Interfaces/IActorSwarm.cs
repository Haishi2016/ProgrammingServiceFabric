using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;
using System.Threading.Tasks;

[assembly: FabricTransportActorRemotingProvider(RemotingListener = RemotingListener.V2Listener, RemotingClient = RemotingClient.V2Client)]
namespace ActorSwarm.Interfaces
{
    public interface IActorSwarm : IActor
    {
        Task InitializeAsync(int size, float probability);
        Task EvolveAsync();
        Task<string> ReadStateStringAsync();
    }
}

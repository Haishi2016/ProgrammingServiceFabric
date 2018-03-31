using Microsoft.Owin.Hosting;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Fabric;
using System.Fabric.Description;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using WebCalculatorService.Interfaces;

namespace WebCalculatorService
{
    public class OwinCommunicationListener : ICommunicationListener
    {
        private readonly IOwinAppBuilder startup;
        private readonly string appRoot;
        private IDisposable serverHandle;
        private string listeningAddress;
        private readonly StatelessServiceContext parameters;

        public OwinCommunicationListener(string appRoot, IOwinAppBuilder startup, StatelessServiceContext parameters)
        {
            this.appRoot = appRoot;
            this.startup = startup;
            this.parameters = parameters;
        }

        /* Cancel and stop immediately. */
        public void Abort()
        {
            this.StopWebServer();
        }

        /* Stop taking new requests, finish pending requests, and stop. */
        public Task CloseAsync(CancellationToken cancellationToken)
        {
            this.StopWebServer();
            return Task.FromResult(true);
        }

        /* Set up the address that the server will be listening on and start taking requests. */
        /* 
          OpenAsync method starts the web server and returns the address that the server
          listens on. Service Fabric registers the returned address with its naming service so that a client
          can query for the address by service name. This is necessary because a service instance may get
          moved for load balancing or fail over, hence the listening address could change. However, a client
          always can use the stable service name to look up the correct address to talk to.
         */
        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            EndpointResourceDescription serviceEndpoint = parameters.CodePackageActivationContext.GetEndpoint("ServiceEndpoint");
            int port = serviceEndpoint.Port;
            listeningAddress = string.Format(
                CultureInfo.InvariantCulture,
                "http://+:{0}/{1}",
                port,
                string.IsNullOrWhiteSpace(appRoot) ? string.Empty : appRoot.TrimEnd('/') + '/');
            this.serverHandle = WebApp.Start(this.listeningAddress,
                appBuilder => this.startup.Configuration(appBuilder));
            string resultAddress = this.listeningAddress.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);
            ServiceEventSource.Current.Message("Listening on {0}", resultAddress);
            return Task.FromResult(resultAddress);
        }

        private void StopWebServer()
        {
            if (this.serverHandle != null)
            {
                try
                {
                    this.serverHandle.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // no-op
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Common;
using Microsoft.ServiceFabric.Actors.Client;
using ProductActor.Interfaces;
using Microsoft.ServiceFabric.Actors;

namespace Gateway
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class Gateway : StatelessService, IWebSocketConnectionHandler
    {
        public Gateway(StatelessServiceContext context)
            : base(context)
        { }

        public async Task<byte[]> ProcessWsMessageAsync(byte[] wsrequest, CancellationToken cancellationToken)
        {
            ProtobufWsSerializer mserializer = new ProtobufWsSerializer();

            WsRequestMessage mrequest = await mserializer.DeserializeAsync<WsRequestMessage>(wsrequest);

            switch (mrequest.Operation)
            {
                case "sell":
                    {
                        IWsSerializer pserializer = SerializerFactory.CreateSerializer();
                        PostSalesModel payload = await pserializer.DeserializeAsync<PostSalesModel>(mrequest.Value);
                        var id = payload.Country + "-" + payload.Product;
                        var product = ActorProxy.Create<IProductActor>(new ActorId(id), "fabric:/ECommerceApp");
                        await product.SellAsync();
                    }
                    break;
            }

            WsResponseMessage mresponse = new WsResponseMessage
            {
                Result = WsResult.Success
            };

            return await mserializer.SerializeAsync(mresponse);
        }


        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]
                      {
                new ServiceInstanceListener(
                    initParams => new WebSocketListener("SalesServiceWS", this.Context, () => this),
                    "Websocket")
            };
        }
    }
}

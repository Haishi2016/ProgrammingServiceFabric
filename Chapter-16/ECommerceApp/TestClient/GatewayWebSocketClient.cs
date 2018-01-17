using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestClient
{
    public class GatewayWebSocketClient : IDisposable
    {
        WebSocketManager websocketManager = null;

        public GatewayWebSocketClient()
        {
        }

        public void Dispose()
        {
            try
            {
                this.CloseAsync().Wait();
            }
            catch (ObjectDisposedException)
            {
            }
            catch (AggregateException ae)
            {
                ae.Handle(ex => { return true; });
            }
            finally
            {
                this.websocketManager = null;
            }
        }

        public async Task<bool> ConnectAsync(string serviceName)
        {
            Uri serviceAddress = new Uri(new Uri(serviceName), "Websocket1");

            return await this.ConnectAsync(serviceAddress);
        }

        public async Task<bool> ConnectAsync(Uri serviceAddress)
        {
            if (this.websocketManager == null)
                this.websocketManager = new WebSocketManager();

            return await this.websocketManager.ConnectAsync(serviceAddress);
        }

        public async Task CloseAsync()
        {
            try
            {
                if (this.websocketManager != null)
                {
                    await this.websocketManager.CloseAsync();
                    this.websocketManager.Dispose();
                }
            }
            finally
            {
                this.websocketManager = null;
            }
        }

        /// <summary>
        /// Re-uses the open websocket connection (assumes one is already created/connected)
        /// </summary>
        public async Task<WsResponseMessage> SendReceiveAsync(WsRequestMessage msgspec, CancellationToken cancellationToken)
        {
            if (this.websocketManager == null)
                throw new ApplicationException("SendReceiveAsync requires an open websocket client");

            // Serialize Msg payload
            IWsSerializer mserializer = new ProtobufWsSerializer();

            byte[] request = await mserializer.SerializeAsync(msgspec);

            byte[] response = await this.websocketManager.SendReceiveAsync(request);

            return await mserializer.DeserializeAsync<WsResponseMessage>(response);
        }
    }
}

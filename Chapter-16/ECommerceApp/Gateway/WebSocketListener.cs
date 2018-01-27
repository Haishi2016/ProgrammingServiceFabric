using Common;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gateway
{
    public class WebSocketListener : ICommunicationListener
    {
        private readonly StatelessServiceContext serviceContext;
        private string listeningAddress;
        private readonly string appRoot;
        private string publishAddress;
        private WebSocketApp webSocketApp;
        private Task mainLoop;
        private const int MaxBufferSize = 102400;
        private readonly Func<IWebSocketConnectionHandler> createConnectionHandler;
        public WebSocketListener(
            string appRoot,
            StatelessServiceContext serviceContext,
            Func<IWebSocketConnectionHandler> createConnectionHandler)
        {
            this.appRoot = appRoot;
            this.serviceContext = serviceContext;
            this.createConnectionHandler = createConnectionHandler;
        }
        public void Abort()
        {
            this.StopAll();
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            this.StopAll();
            return Task.FromResult(true);
        }

        public async Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            EndpointResourceDescription endpoint = this.serviceContext
                .CodePackageActivationContext.GetEndpoint("ServiceEndpoint");
            int port = endpoint.Port;
            this.listeningAddress = string.Format("http://+:{0}/{1}", port, appRoot);
            this.publishAddress = this.listeningAddress.Replace(
                "+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);
            this.publishAddress = this.publishAddress.Replace("http", "ws");
            this.webSocketApp = new WebSocketApp(this.listeningAddress);
            this.webSocketApp.Init();
            this.mainLoop = this.webSocketApp.StartAsync(this.ProcessConnectionAsync);
            return await Task.FromResult(this.publishAddress);
        }
        private async Task<bool> ProcessConnectionAsync(
            CancellationToken cancellationToken,
            HttpListenerContext httpContext)
        {
            WebSocketContext webSocketContext = null;
            try
            {
                webSocketContext = await httpContext.AcceptWebSocketAsync(null);
            }
            catch
            {
                httpContext.Response.StatusCode = 500;
                httpContext.Response.Close();
                return false;
            }
            WebSocket webSocket = webSocketContext.WebSocket;
            MemoryStream ms = new MemoryStream();
            IWebSocketConnectionHandler handler = this.createConnectionHandler();
            byte[] receiveBuffer = null;
            while (webSocket.State == WebSocketState.Open)
            {
                if (receiveBuffer == null)
                    receiveBuffer = new byte[MaxBufferSize];
                WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), cancellationToken);
                if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancellationToken);
                    continue;
                }
                if (receiveResult.EndOfMessage)
                {
                    await ms.WriteAsync(receiveBuffer, 0, receiveResult.Count, cancellationToken);
                    receiveBuffer = ms.ToArray();
                    ms.Dispose();
                    ms = new MemoryStream();
                }
                else
                {
                    await ms.WriteAsync(receiveBuffer, 0, receiveResult.Count, cancellationToken);
                    continue;
                }
                byte[] wsresponse = null;
                try
                {
                    wsresponse = await handler.ProcessWsMessageAsync(receiveBuffer, cancellationToken);
                }
                catch (Exception ex)
                {
                    wsresponse = await new ProtobufWsSerializer().SerializeAsync(
                        new WsResponseMessage
                        {
                            Result = WsResult.Error,
                            Value = Encoding.UTF8.GetBytes(ex.Message)
                        });
                }

                await webSocket.SendAsync(
                    new ArraySegment<byte>(wsresponse),
                    WebSocketMessageType.Binary,
                    true,
                    cancellationToken);
            }
            return true;
        }
        private void StopAll()
        {
            try
            {
                this.webSocketApp.Dispose();
                if (this.mainLoop != null)
                {
                    this.mainLoop.Wait(TimeSpan.FromSeconds(3));
                    this.mainLoop.Dispose();
                    this.mainLoop = null;
                }
                this.listeningAddress = string.Empty;
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }
}

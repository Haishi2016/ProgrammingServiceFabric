using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestClient
{
    public class WebSocketManager : IDisposable
    {
        const int MaxBufferSize = 10240;
        ClientWebSocket clientWebSocket = null;
        byte[] receiveBytes = new byte[MaxBufferSize];

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
        }

        public async Task<bool> ConnectAsync(Uri serviceAddress)
        {
            this.clientWebSocket = new ClientWebSocket();

            using (CancellationTokenSource tcs = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                await this.clientWebSocket.ConnectAsync(serviceAddress, tcs.Token);
            }

            return true;
        }

        public async Task<byte[]> SendReceiveAsync(byte[] payload)
        {
            try
            {
                // Send request operation
                await this.clientWebSocket.SendAsync(new ArraySegment<byte>(payload), WebSocketMessageType.Binary, true, CancellationToken.None);

                WebSocketReceiveResult receiveResult =
                    await this.clientWebSocket.ReceiveAsync(new ArraySegment<byte>(this.receiveBytes), CancellationToken.None);

                using (MemoryStream ms = new MemoryStream())
                {
                    await ms.WriteAsync(this.receiveBytes, 0, receiveResult.Count);
                    return ms.ToArray();
                }
            }
            catch
            {
                this.CloseAsync().Wait();
            }

            return null;
        }

        public async Task CloseAsync()
        {
            try
            {
                if (this.clientWebSocket != null)
                {
                    if (this.clientWebSocket.State != WebSocketState.Closed)
                        await this.clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    this.clientWebSocket.Dispose();
                }
            }
            finally
            {
                this.clientWebSocket = null;
            }
        }
    }
}

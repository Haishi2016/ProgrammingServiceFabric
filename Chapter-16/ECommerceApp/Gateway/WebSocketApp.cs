using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gateway
{
    public class WebSocketApp : IDisposable
    {
        private static readonly byte[] UncaughtHttpBytes =
            Encoding.Default.GetBytes("Uncaught error in main processing loop!");
        private string address;
        private CancellationToken cancellationToken;
        private CancellationTokenSource cancellationTokenSource;
        private HttpListener httpListener;
        public WebSocketApp(string address)
        {
            this.address = address;
            this.cancellationTokenSource = new CancellationTokenSource();
        }
        public void Dispose()
        {
            try
            {
                if (this.cancellationTokenSource != null && !this.cancellationTokenSource.IsCancellationRequested)
                    this.cancellationTokenSource.Cancel();

                if (this.httpListener != null && this.httpListener.IsListening)
                {
                    this.httpListener.Stop();
                    this.httpListener.Close();
                }

                if (this.cancellationTokenSource != null && !this.cancellationTokenSource.IsCancellationRequested)
                    this.cancellationTokenSource.Dispose();
            }
            catch (ObjectDisposedException)
            {
            }
            catch (AggregateException ae)
            {
                ae.Handle(ex => { return true; });
            }
        }
        public void Init()
        {
            if (!this.address.EndsWith("/"))
                this.address += "/";

            this.httpListener = new HttpListener();
            this.httpListener.Prefixes.Add(this.address);
            this.cancellationTokenSource = new CancellationTokenSource();
            this.cancellationToken = this.cancellationTokenSource.Token;
            this.httpListener.Start();
        }
        public async Task StartAsync(
            Func<CancellationToken, HttpListenerContext, Task<bool>> processActionAsync
            )
        {
            while (this.httpListener.IsListening)
            {
                HttpListenerContext context = null;
                try
                {
                    context = await this.httpListener.GetContextAsync();
                }
                catch (Exception ex)
                {
                    if (this.cancellationToken.IsCancellationRequested)
                        return;
                    continue;
                }

                if (this.cancellationToken.IsCancellationRequested)
                    return;

                this.DispatchConnectedContext(context, processActionAsync);
            }
        }
        private void DispatchConnectedContext(
            HttpListenerContext context,
            Func<CancellationToken, HttpListenerContext, Task<bool>> processActionAsync
            )
        {
            // do not await on processAction since we don't want to block on waiting for more connections
            processActionAsync(this.cancellationToken, context)
                .ContinueWith(
                    t =>
                    {
                        if (t.IsFaulted)
                        {
                            context.Response.ContentLength64 = UncaughtHttpBytes.Length;
                            context.Response.StatusCode = 500;
                            context.Response.OutputStream.Write(UncaughtHttpBytes, 0, UncaughtHttpBytes.Length);
                            context.Response.OutputStream.Close();
                        }
                    },
                    this.cancellationToken);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gateway
{
    public interface IWebSocketConnectionHandler
    {
        Task<byte[]> ProcessWsMessageAsync(byte[] wsrequest, CancellationToken cancellationToken);
    }
}

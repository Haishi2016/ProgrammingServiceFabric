using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace WordCounter.Interfaces
{
    public interface IWordCounter : IActor
    { 
        Task CountWordsAsync(string sentence, CancellationToken cancellationToken);
    }
}

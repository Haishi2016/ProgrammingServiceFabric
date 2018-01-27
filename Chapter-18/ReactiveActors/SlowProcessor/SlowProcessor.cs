using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using SlowProcessor.Interfaces;

namespace SlowProcessor
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class SlowProcessor : Actor, ISlowProcessor
    {
        public SlowProcessor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");
            return this.StateManager.TryAddStateAsync("progress", 0);
        }
        
        public Task<int> GetJobProgressAsync(CancellationToken cancellationToken)
        {
            return this.StateManager.GetStateAsync<int>("progress", cancellationToken);
        }

        public async Task SetJobAsync(string job, CancellationToken cancellationToken)
        {
            for (int i = 0; i <= 100; i++)
            {
                await this.StateManager.SetStateAsync<int>("progress", i, cancellationToken);
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using ProductActor.Interfaces;
using System.Runtime.Serialization;
using System.Globalization;
using System.ComponentModel;

namespace ProductActor
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class ProductActor : Actor, IProductActor
    {
        [DataContract]
        internal sealed class ActorState
        {
            [DataMember]
            public int Sales { get; set; }

            public override string ToString()
            {
                return string.Format(CultureInfo.InvariantCulture, "ProductActor.ActorState[Sales = {0}]", Sales);
            }
        }
        public ProductActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        protected override Task OnActivateAsync()
        {
            var state = new ActorState { Sales = 0 };
            ActorEventSource.Current.ActorMessage(this, "State initialized to {0}", state);
            return this.StateManager.SetStateAsync<ActorState>("Sales", state);
        }

        [ReadOnly(true)]
        async Task<int> IProductActor.GetSalesAsync()
        {
            var state = await this.StateManager.GetStateAsync<ActorState>("Sales");
            ActorEventSource.Current.ActorMessage(this, "Getting current sales value as {0}", state.Sales);
            return state.Sales;
        }
        
        Task IProductActor.SellAsync()
        {
            return this.StateManager.AddOrUpdateStateAsync<ActorState>("Sales", new ActorState { Sales = 0 }, 
                (key, value) => new ActorState { Sales = value.Sales + 1 });
        }
    }
}

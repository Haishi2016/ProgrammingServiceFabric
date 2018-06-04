using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using Termite.Interfaces;
using Box.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace Termite
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
    internal class Termite : Actor, ITermite
    {
        #region Private vars
        private IActorTimer mTimer;
        private static Random rand = new Random();
        private string actorStateName = "TermiteActorState";
        private const int size = 100;
        #endregion

        /// <summary>
        /// Initializes a new instance of Termite
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public Termite(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        public Task<TermiteState> GetStateAsync()
        {
            var state = this.StateManager.TryGetStateAsync<TermiteState>(actorStateName);
            if (state != null && state.Result.HasValue)
                return Task.FromResult(state.Result.Value);
            return Task.FromResult<TermiteState>(null);
        }

        public Task SetStateAsync<T>(T state)
        {
            try
            {
                this.StateManager.SetStateAsync<T>(actorStateName, state);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            if (this.GetStateAsync().Result == null)
            {
                this.StateManager.SetStateAsync(actorStateName, new TermiteState()
                {
                    X = rand.Next(0, size),
                    Y = rand.Next(0, size),
                    HasWoodChip = false
                });

                var state = this.GetStateAsync().Result;
                mTimer = RegisterTimer(Move, state, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(50));
            }

            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see https://aka.ms/servicefabricactorsstateserialization

            return Task.FromResult(true);
        }

        protected override Task OnDeactivateAsync()
        {
            if (mTimer != null)
                UnregisterTimer(mTimer);

            return base.OnDeactivateAsync();
        }

        private async Task Move(object state)
        {
            TermiteState tState = state as TermiteState;

            IBox boxClient = ServiceProxy.Create<IBox>(new Uri("fabric:/TermiteModel/Box"),
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(rand.Next()));

            if (!tState.HasWoodChip)
            {
                var result = await boxClient.TryPickUpWoodChipAsync(tState.X, tState.Y);
                if (result)
                {
                    tState.HasWoodChip = true;
                }
            }
            else
            {
                var result = await boxClient.TryPutDownWoodChipAsync(tState.X, tState.Y);
                if (result)
                {
                    tState.HasWoodChip = false;
                }
            }

            int action = rand.Next(1, 9);
            //1-left; 2-left-up; 3-up; 4-up-right; 5-right: 6-right-down; 7-down; 8-down-left
            if ((action == 1 || action == 2 || action == 8) && tState.X > 0)
                tState.X = tState.X - 1;
            if ((action == 4 || action == 5 || action == 6) && tState.X < size - 1)
                tState.X = tState.X + 1;
            if ((action == 2 || action == 3 || action == 4) && tState.Y > 0)
                tState.Y = tState.Y - 1;
            if ((action == 6 || action == 7 || action == 8) && tState.Y < size - 1)
                tState.Y = tState.Y + 1;

            await this.SetStateAsync(state);
            await this.SaveStateAsync();
        }
    }
}

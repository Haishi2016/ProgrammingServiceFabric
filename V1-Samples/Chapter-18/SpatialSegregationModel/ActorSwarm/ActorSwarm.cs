using ActorSwarm.Interfaces;
using Common;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActorSwarm
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
    [ActorService(Name = "SpatialSwarm")]
    internal class ActorSwarm : Actor, IActorSwarm
    {
        private string actorStateName = "SwarmState";
        int mSize = 100;
        static Random mRand = new Random();

        /// <summary>
        /// Initializes a new instance of ActorSwarm
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public ActorSwarm(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        public async Task EvolveAsync()
        {
            var state = await this.GetStateAsync();
            foreach (var actor in state.VirtualActors)
            {
                state.SharedState.Propose(await actor.ProposeAsync());
            }

            state.SharedState.ResolveConflictsAndCommit((p) =>
            {
                if (p is Proposal2D<byte> proposal)
                {
                    state.VirtualActors[proposal.ActorId].ApproveProposalAsync(proposal);
                }
            });
        }

        public async Task InitializeAsync(int size, float probability)
        {
            await this.InitializeStateAsync();

            var state = await this.GetStateAsync();
            state.SharedState.Initialize(mSize);

            int count = (int)(size * size * probability);
            for (int i = 0; i < count; i++)
            {
                state.VirtualActorStates.Add(
                    new ResidentState
                    {
                        X = mRand.Next(0, size),
                        Y = mRand.Next(0, size),
                        Tag = (byte)mRand.Next(1, 3)
                    });
                state.VirtualActors.Add(
                    new Resident(size, i, state.VirtualActorStates[i], state.SharedState)
                    );
            }
            await this.SetStateAsync(state);

            return;
        }

        public async Task<string> ReadStateStringAsync()
        {
            var state = await this.GetStateAsync();
            return state.SharedState.ToString();
        }

        public Task<SwarmState> GetStateAsync()
        {
            var state = this.StateManager.TryGetStateAsync<SwarmState>(actorStateName);
            if (state != null && state.Result.HasValue)
                return Task.FromResult(state.Result.Value);
            return Task.FromResult<SwarmState>(null);
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

        public Task InitializeStateAsync()
        {
            this.StateManager.SetStateAsync(actorStateName, new SwarmState()
            {
                SharedState = new Shared2DArray<byte>(),
                VirtualActorStates = new List<ResidentState>(),
                VirtualActors = new List<IVirtualActor>()
            });

            return Task.FromResult(true);
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            if (this.GetStateAsync().Result == null)
            {
                this.StateManager.SetStateAsync(actorStateName, new SwarmState()
                {
                    SharedState = new Shared2DArray<byte>(),
                    VirtualActorStates = new List<ResidentState>(),
                    VirtualActors = new List<IVirtualActor>()
                });
            }

            ActorEventSource.Current.ActorMessage(this, "State inizialized.");

            return Task.FromResult(true);
        }

    }
}

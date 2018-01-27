using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using ApplicationActor.Interfaces;
using System.Fabric;
using System.Fabric.Description;

namespace ApplicationActor
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
    internal class ApplicationActor : Actor, IApplicationActor, IRemindable
    {
        /// <summary>
        /// Initializes a new instance of ApplicationActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
public ApplicationActor(ActorService actorService, ActorId actorId)
    : base(actorService, actorId)
{
    this.RegisterReminderAsync("MyAppReminder", null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));
}

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see https://aka.ms/servicefabricactorsstateserialization

            return this.StateManager.TryAddStateAsync("count", 0);
        }

        /// <summary>
        /// TODO: Replace with your own actor method.
        /// </summary>
        /// <returns></returns>
        public async Task DeleteTenantAsync(CancellationToken cancellationToken)
        {
            await this.StateManager.AddOrUpdateStateAsync<ApplicationState>("Desired",
                new ApplicationState { AppType = "", AppVersion = "", IsRunning = false, TenantName = "" },
                (k, v) =>
                {
                    return new ApplicationState
                    {
                        AppType = v.AppType,
                        AppVersion = v.AppVersion,
                        TenantName = v.TenantName,
                        IsRunning = false
                    };
                }, cancellationToken);            
        }

        public async Task CreateTenantAsync(ApplicationState state, CancellationToken cancellationToken)
        {
            await this.StateManager.AddOrUpdateStateAsync<ApplicationState>("Desired",
                new ApplicationState
                {
                    AppType = state.AppType,
                    AppVersion = state.AppVersion,
                    IsRunning = true,
                    TenantName = state.TenantName
                },
                (k, v) =>
                {
                    return new ApplicationState
                    {
                        AppType = v.AppType,
                        AppVersion = v.AppVersion,
                        TenantName = v.TenantName,
                        IsRunning = false
                    };
                }, cancellationToken);
        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            FabricClient client = new FabricClient("localhost:19000");
            var desiredState = await this.StateManager.TryGetStateAsync<ApplicationState>("Desired");
            if (desiredState.HasValue) {
                var health = await client.HealthManager.GetApplicationHealthAsync(new Uri(this.Id.GetStringId()));
                var healthState = health.AggregatedHealthState;
                if (desiredState.Value.IsRunning == true)
                {
                    if (healthState == System.Fabric.Health.HealthState.Invalid)
                    {
                        await client.ApplicationManager.CreateApplicationAsync(new System.Fabric.Description.ApplicationDescription
                        {
                            ApplicationTypeName = desiredState.Value.AppType,
                            ApplicationTypeVersion = desiredState.Value.AppVersion,
                            ApplicationName = new Uri(desiredState.Value.TenantName)
                        });
                    }
                }
                else
                {
                    await client.ApplicationManager.DeleteApplicationAsync(new DeleteApplicationDescription(new Uri(this.Id.GetStringId()))
                    {
                        ForceDelete = true
                    });
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using CarActor.Interfaces;

namespace CarActor
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class CarActor : Actor, ICarActor
    {
        IActorTimer mTimer;
        
        public CarActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");
            mTimer = this.RegisterTimer(Move,    //callback function
                        null,   //callback state
                        TimeSpan.FromSeconds(5),  //delay before first callback
                        TimeSpan.FromSeconds(1));   //callback interval
            return this.StateManager.TryAddStateAsync("My-Location", new Tuple<double,double>(-122.041934,47.694861));
        }
        private async Task Move(Object state)
        {
            var location = await this.StateManager.GetStateAsync<Tuple<double, double>>("My-Location");
            ActorEventSource.Current.ActorMessage(this, "Car is at (" 
                + location.Item1 + "," + location.Item2 + ")");
            var longitude = location.Item1 + 0.01;
            if (longitude > 180.0)
                longitude = -180.0;
            await this.StateManager.SetStateAsync("My-Location", 
                new Tuple<double, double>(longitude, location.Item2));
        }

        protected override Task OnDeactivateAsync()
        {
            if (mTimer != null)
                UnregisterTimer(mTimer);
            return base.OnDeactivateAsync();
        }
        Task<Tuple<double,double>> ICarActor.GetLocationAsync(CancellationToken cancellationToken)
        {
            return this.StateManager.GetStateAsync<Tuple<double,double>>("My-Location", cancellationToken);
        }
        Task ICarActor.SetLocationAsync(Tuple<double,double> location, CancellationToken cancellationToken)
        {
             return this.StateManager.SetStateAsync("My-Locaiton", location, cancellationToken);
        }
    }
}

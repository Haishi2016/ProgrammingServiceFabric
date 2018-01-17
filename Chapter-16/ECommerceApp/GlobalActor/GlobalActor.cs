using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using GlobalActor.Interfaces;
using System.Collections.Concurrent;
using CountryActor.Interfaces;

namespace GlobalActor
{   
    [StatePersistence(StatePersistence.Persisted)]
    internal class GlobalActor : Actor, IGlobalActor
    {
        DateTime mLastUpdate = DateTime.MinValue;
        public GlobalActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        async Task<List<Tuple<string, long>>> IGlobalActor.CountGlobalSalesAsync()
        {
            if (DateTime.Now - mLastUpdate > TimeSpan.FromSeconds(1))
            {
                string[] countries = { "US", "China", "Australia" };
                ConcurrentDictionary<string, long> sales = new ConcurrentDictionary<string, long>();
                Parallel.ForEach(countries, country =>
                {
                    var proxy = ActorProxy.Create<ICountryActor>(new ActorId(country), "fabric:/ECommerceApp");
                    try
                    {
                        var countrySales = proxy.CountCountrySalesAsync().Result;
                        if (countrySales != null)
                        {
                            foreach (var tuple in countrySales)
                            {
                                sales.AddOrUpdate(tuple.Item1, tuple.Item2, (key, oldValue) => oldValue + tuple.Item2);
                            }
                        }
                    }
                    catch {//ignore some transient errors}
                });
                var list = (from entry in sales
                            orderby entry.Value descending
                            select new Tuple<string, long>(entry.Key, entry.Value)).ToList();
                await this.StateManager.SetStateAsync<List<Tuple<string, long>>>("GlobalState", list);
                mLastUpdate = DateTime.Now;
                return list;
            } else
            {
                var result = await this.StateManager.TryGetStateAsync<List<Tuple<string, long>>>("GlobalState");
                if (result.HasValue)
                    return result.Value;
                else
                    return new List<Tuple<string, long>>();
            }
           
        }
    }
}

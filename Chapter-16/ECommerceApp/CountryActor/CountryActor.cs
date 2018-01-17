using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using CountryActor.Interfaces;
using ProductActor.Interfaces;

namespace CountryActor
{
    [StatePersistence(StatePersistence.None)]
    internal class CountryActor : Actor, ICountryActor
    {
        DateTime mLastUpdate = DateTime.MinValue;
        public CountryActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        async Task<List<Tuple<string, long>>> ICountryActor.CountCountrySalesAsync()
        {
            if (DateTime.Now - mLastUpdate > TimeSpan.FromSeconds(1))
            {
                string[] products = { "VCR", "Fax", "CassettePlayer", "Camcorder", "GameConsole",
                            "CD", "TV", "Radio", "Phone", "Karaoke"};
                List<Tuple<string, long>> ret = new List<Tuple<string, long>>();
                Parallel.ForEach(products, product =>
                {
                    string actorId = this.Id.GetStringId() + "-" + product;
                    var proxy = ActorProxy.Create<IProductActor>(new ActorId(actorId), "fabric:/ECommerceApp");
                    try
                    {
                        ret.Add(new Tuple<string, long>(product, proxy.GetSalesAsync().Result));
                    }
                    catch {//ignore some transient errors }
                });
                await this.StateManager.SetStateAsync<List<Tuple<string, long>>>("CountryState", ret);
                mLastUpdate = DateTime.Now;
                return ret;
            }
            else
            {
                var result = await this.StateManager.TryGetStateAsync<List<Tuple<string, long>>>("CountryState");
                if (result.HasValue)
                    return result.Value;
                else
                    return new List<Tuple<string, long>>();
            }
        }
    }
}

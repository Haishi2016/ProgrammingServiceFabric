using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using StateAggregator.Interfaces;
using Microsoft.ServiceFabric.Services.Communication.Wcf;

namespace StateAggregator
{
    internal sealed class StateAggregator : StatefulService, IStateAggregator
    {
        public StateAggregator(StatefulServiceContext context)
            : base(context)
        { }

        public async Task<List<JobStatus>> ListJobs()
        {
            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, JobStatus>>("myDictionary");
            using (var tx = this.StateManager.CreateTransaction())
            {
                List<JobStatus> ret = new List<JobStatus>();
                var enumerable = await myDictionary.CreateEnumerableAsync(tx);
                using (var e = enumerable.GetAsyncEnumerator())
                {
                    while (await e.MoveNextAsync(default(CancellationToken)).ConfigureAwait(false))
                    {
                        //var job = e.Current.Value;
                        //if (job.Submitter.IndexOf("#") > 0)
                        //    job.Submitter = job.Submitter.Substring(job.Submitter.IndexOf("#")+1).Trim();
                        ret.Add(e.Current.Value);
                    }
                }
                await tx.CommitAsync();

                return (from j in ret orderby j.Date descending select j).ToList();
            }
        }

        public async Task ReportCompletion(string name, string url, string user)
        {
            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, JobStatus>>("myDictionary");
            using (var tx = this.StateManager.CreateTransaction())
            {
                await myDictionary.AddOrUpdateAsync(tx, name,
                    new JobStatus { Name = name, Url = url, Submitter = user},
                    (key, val) => {
                        val.Url = url;
                        val.Message = "";
                        val.Percent = 100;
                        val.EndDate = DateTime.UtcNow;
                        val.Submitter = user;
                        return val;
                    });
                await tx.CommitAsync();
            }
        }
        public async Task ReportCancellation(string name)
        {
            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, JobStatus>>("myDictionary");
            using (var tx = this.StateManager.CreateTransaction())
            {
                await myDictionary.TryRemoveAsync(tx, name);
                await tx.CommitAsync();
            }
        }
        public async Task ReportProgress(string name, int percent, string message, string user)
        {
            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, JobStatus>>("myDictionary");
            using (var tx = this.StateManager.CreateTransaction())
            {
                await myDictionary.AddOrUpdateAsync(tx, name,
                    new JobStatus {
                        Name = name,
                        Message = message,
                        Percent = percent,
                        EndDate = DateTime.UtcNow,
                        Date = DateTime.UtcNow,
                        Submitter = user
                    },
                    (key, val) => {
                        val.Percent = percent;
                        if (!string.IsNullOrEmpty(message))
                            val.Message = message;
                        return val;
                    });
                await tx.CommitAsync();
            }
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
            {
                new ServiceReplicaListener((context)=>
                    new WcfCommunicationListener<IStateAggregator>(
                        wcfServiceObject:this,
                        serviceContext:context,
                        endpointResourceName: "ServiceEndpoint",
                        listenerBinding: WcfUtility.CreateTcpListenerBinding()
                        ))
            };
        }
    }
}

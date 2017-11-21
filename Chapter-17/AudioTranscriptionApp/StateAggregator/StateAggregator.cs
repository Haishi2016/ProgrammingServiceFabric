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
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
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
                        ret.Add(e.Current.Value);
                }
                await tx.CommitAsync();
                return ret;
            }
        }

        public async Task ReportCompletion(string name, string url)
        {
            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, JobStatus>>("myDictionary");
            using (var tx = this.StateManager.CreateTransaction())
            {
                await myDictionary.AddOrUpdateAsync(tx, name,
                    new JobStatus { Name = name, Url = url },
                    (key, val) => {
                        val.Url = url;
                        return val;
                    });
                await tx.CommitAsync();
            }
        }

        public async Task ReportProgress(string name, int percent, string message)
        {
            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, JobStatus>>("myDictionary");
            using (var tx = this.StateManager.CreateTransaction())
            {
                await myDictionary.AddOrUpdateAsync(tx, name,
                    new JobStatus { Name = name, Message = message, Precent = percent },
                    (key, val) => {
                        val.Precent = percent;
                        val.Message = message;
                        return val;
                    });
                await tx.CommitAsync();
            }
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
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

        ///// <summary>
        ///// This is the main entry point for your service replica.
        ///// This method executes when this replica of your service becomes primary and has write status.
        ///// </summary>
        ///// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        //protected override async Task RunAsync(CancellationToken cancellationToken)
        //{
        //    // TODO: Replace the following sample code with your own logic 
        //    //       or remove this RunAsync override if it's not needed in your service.

        //    var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

        //    while (true)
        //    {
        //        cancellationToken.ThrowIfCancellationRequested();

        //        using (var tx = this.StateManager.CreateTransaction())
        //        {
        //            var result = await myDictionary.TryGetValueAsync(tx, "Counter");

        //            ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
        //                result.HasValue ? result.Value.ToString() : "Value does not exist.");

        //            await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

        //            // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
        //            // discarded, and nothing is saved to the secondary replicas.
        //            await tx.CommitAsync();
        //        }

        //        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        //    }
        //}
    }
}

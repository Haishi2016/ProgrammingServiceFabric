using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Box.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;

namespace Box
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class Box : StatefulService, IBox
    {
        #region Private vars
        private static Random mRand = new Random();
        private const string mDictionaryName = "box";
        private const int size = 100;
        #endregion

        public Box(StatefulServiceContext context)
            : base(context)
        { }

        public async Task<List<int>> ReadBox()
        {
            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, int>>(mDictionaryName);
            List<int> ret = new List<int>();

            using (var tx = this.StateManager.CreateTransaction())
            {
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        var value = await myDictionary.TryGetValueAsync(tx, x + "-" + y);
                        if (value.HasValue)
                            ret.Add(value.Value);
                        else
                            ret.Add(0);
                    }
                }
                await tx.CommitAsync();
            }
            return ret;
        }

        public async Task ResetBox()
        {
            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, int>>(mDictionaryName);
            await myDictionary.ClearAsync();

            using (var tx = this.StateManager.CreateTransaction())
            {
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        await myDictionary.SetAsync(tx, x + "-" + y, 0);
                    }
                }

                for (int i = 0; i < (size * size) / 4; i++)
                {
                    var x = mRand.Next(0, size);
                    var y = mRand.Next(0, size);
                    await myDictionary.SetAsync(tx, x + "-" + y, 1);
                }

                await tx.CommitAsync();
            }
        }

        public async Task<bool> TryPickUpWoodChipAsync(int x, int y)
        {
            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, int>>(mDictionaryName);
            var ret = false;

            using (var tx = this.StateManager.CreateTransaction())
            {
                string key = x + "-" + y;
                var result = await myDictionary.TryGetValueAsync(tx, key);
                if (result.HasValue && result.Value == 1)
                {
                    ret = await myDictionary.TryUpdateAsync(tx, key, 0, 1);
                }
                await tx.CommitAsync();
            }
            return ret;
        }

        public async Task<bool> TryPutDownWoodChipAsync(int x, int y)
        {
            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, int>>(mDictionaryName);
            var ret = false;

            int lastX = x;
            int lastY = y;

            using (var tx = this.StateManager.CreateTransaction())
            {
                string key = x + "-" + y;
                var result = await myDictionary.TryGetValueAsync(tx, key);
                if (result.HasValue && result.Value == 1)
                {
                    for (int r = 1; r <= 2; r++)
                    {
                        double angle = mRand.NextDouble() * Math.PI * 2;
                        for (double a = angle; a < Math.PI * 2 + angle; a += 0.1)
                        {
                            int newX = (int)(x + r * Math.Cos(a));
                            int newY = (int)(y + r * Math.Sin(a));

                            if ((newX != lastX || newY != lastY)
                                    && newX >= 0 && newY >= 0 && newX < size && newY < size)
                            {
                                lastX = newX;
                                lastY = newY;

                                string testKey = newX + "-" + newY;
                                var neighbour = await myDictionary.TryGetValueAsync(tx, testKey);
                                if (neighbour.HasValue && neighbour.Value == 0)
                                {
                                    ret = await myDictionary.TryUpdateAsync(tx, testKey, 1, 0);
                                    if (ret)
                                        break;
                                }
                            }
                        }
                        if (ret)
                            break;
                    }
                }
                await tx.CommitAsync();
            }
            return ret;
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
            return new[] { new ServiceReplicaListener(context => this.CreateServiceRemotingListener(context)) };
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {

        }
    }
}

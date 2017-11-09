using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Chaos.DataStructures;
using System.Fabric.Testability.Scenario;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ChaosTest
{
    class ChaosEventComparer : IEqualityComparer<ChaosEvent>
    {
        public bool Equals(ChaosEvent x, ChaosEvent y)
        {
            return x.TimeStampUtc.Equals(y.TimeStampUtc);
        }

        public int GetHashCode(ChaosEvent obj)
        {
            return obj.TimeStampUtc.GetHashCode();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var clusterConnectionString = "localhost:19000";
            using (var client = new FabricClient(clusterConnectionString))
            {
                var startTimeUtc = DateTime.UtcNow;
                var stabilizationTimeout = TimeSpan.FromSeconds(30.0);
                var timeToRun = TimeSpan.FromMinutes(60.0);
                var maxConcurrentFaults = 3;

                var parameters = new ChaosParameters(
                    stabilizationTimeout,
                    maxConcurrentFaults,
                    true, /* EnableMoveReplicaFault */
                    timeToRun);

                try
                {
                    client.TestManager.StartChaosAsync(parameters).GetAwaiter().GetResult();
                }
                catch (FabricChaosAlreadyRunningException)
                {
                    Console.WriteLine("An instance of Chaos is already running in the cluster.");
                }

                var filter = new ChaosReportFilter(startTimeUtc, DateTime.MaxValue);
                var eventSet = new HashSet<ChaosEvent>(new ChaosEventComparer());

                while (true)
                {
                    var report = client.TestManager.GetChaosReportAsync(filter).GetAwaiter().GetResult();               
                    foreach (var chaosEvent in report.History)
                    {
                        if (eventSet.Add(chaosEvent))
                        {
                            Console.WriteLine(chaosEvent);
                        }
                    }
                    var lastEvent = report.History.LastOrDefault();

                    if (lastEvent is StoppedEvent)
                    {
                        break;
                    }              
                    Task.Delay(TimeSpan.FromSeconds(1.0)).GetAwaiter().GetResult();
                }
            }
        }
    }
}

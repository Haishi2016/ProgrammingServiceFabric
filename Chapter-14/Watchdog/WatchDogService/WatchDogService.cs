// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace WatchDogService
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Fabric.Health;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class WatchDogService : StatelessService
    {
        private Uri applicationName = new Uri("fabric:/SidecardDemo");
        private string serviceManifestName = "WatchDogServicePkg";
        private string nodeName = FabricRuntime.GetNodeContext().NodeName;
        private FabricClient Client = new FabricClient(new FabricClientSettings() { HealthReportSendInterval = TimeSpan.FromSeconds(0) });


        public WatchDogService(StatelessServiceContext context)
            : base(context)
        { }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            int failedCount = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        string payload = client.DownloadString(new Uri("http://localhost:8080/"));
                        if (!string.IsNullOrEmpty(payload))
                        {
                            if (payload == "something wrong")
                            {
                                failedCount++;
                                ServiceEventSource.Current.Write("Watchdog had detected " + failedCount + " failures.");
                                if (failedCount >= 5)
                                {
                                    var deployedServicePackageHealthReport = new DeployedServicePackageHealthReport(
                                        applicationName,
                                        serviceManifestName,
                                        nodeName,
                                        new HealthInformation("CustomWatchDog", "MyServiceHealth", HealthState.Warning));
                                    Client.HealthManager.ReportHealth(deployedServicePackageHealthReport);
                                    ServiceEventSource.Current.Write("Watchdog is sad.");
                                }
                            }
                            else
                            {
                                failedCount = 0;
                                var deployedServicePackageHealthReport = new DeployedServicePackageHealthReport(
                                        applicationName,
                                        serviceManifestName,
                                        nodeName,
                                        new HealthInformation("CustomWatchDog", "MyServiceHealth", HealthState.Ok));
                                Client.HealthManager.ReportHealth(deployedServicePackageHealthReport);
                                ServiceEventSource.Current.Write("Watchdog is happy");
                            }
                        }
                    }
                    catch (WebException)
                    {
                        failedCount++;
                        ServiceEventSource.Current.Write("Watchdog had detected " + failedCount + " failures.");
                        if (failedCount >= 5)
                        {
                            var deployedServicePackageHealthReport = new DeployedServicePackageHealthReport(
                                applicationName,
                                serviceManifestName,
                                nodeName,
                                new HealthInformation("CustomWatchDog", "MyServiceHealth", HealthState.Warning));
                            Client.HealthManager.ReportHealth(deployedServicePackageHealthReport);
                            ServiceEventSource.Current.Write("Watchdog is sad.");
                        }
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}

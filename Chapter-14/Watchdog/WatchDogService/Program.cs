// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace WatchDogService
{
    using System;
    using System.Diagnostics.Tracing;
    using System.Fabric;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Runtime;

    /// <summary>
    /// This is the entrypoint for the host process that hosts the service.
    /// </summary>
    public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                //Creating a new event listener to redirect the traces to a file
                ServiceEventListener listener = new ServiceEventListener();
                listener.EnableEvents(ServiceEventSource.Current, EventLevel.LogAlways, EventKeywords.All);

                // The ServiceManifest.XML file defines one or more service type names.
                // Registering a service maps a service type name to a .NET type.
                // When Service Fabric creates an instance of this service type,
                // an instance of the class is created in this host process.
                ServiceEventSource.Current.Message("Registering Service : {0}", "WatchDogService");

                ServiceRuntime.RegisterServiceAsync("WatchDogServiceType",
                    context => new WatchDogService (context)).GetAwaiter().GetResult();

                ServiceEventSource.Current.Message("Registered Service : {0}", "WatchDogService");

                // Prevents this host process from terminating so services keep running.
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(ex);
                throw ex;
            }
        }
    }
}

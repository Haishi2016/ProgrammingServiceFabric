using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Runtime.Serialization;
using System.IO;
using Microsoft.ServiceFabric.Data;
using System.Text;

namespace PersonService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class PersonService : StatefulService
    {
        public PersonService(StatefulServiceContext context)
            : base(context)
        {
            this.StateManager.TryAddStateSerializer(new PersonSerializer());
        }

        private static IReliableStateManager CreateReliableStateManager()
        {
            return null;
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
            
            return new ServiceReplicaListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, Person>>("myDictionary");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Customer");

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                        result.HasValue ? serializePerson(result.Value): "Value does not exist.");

                    await myDictionary.AddOrUpdateAsync(tx, "Customer", new Person { FirstName = "Haishi", LastName = "Bai", ID = 100 },
                        (key, value) => new Person { FirstName = "Haishi", LastName = "Bai", ID = 100 });

                    await tx.CommitAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
        private string serializePerson(Person person)
        {
            //var serializer = new DataContractSerializer(typeof(Person));
            //using (var stream = new StringWriter())
            //{
            //    using (var writer = System.Xml.XmlWriter.Create(stream))
            //    {
            //        serializer.WriteObject(writer, person);
            //    }
            //    return stream.ToString();
            //}

            var serializer = new PersonSerializer();
            using (var stream = new MemoryStream())
            {
                serializer.Write(person, new BinaryWriter(stream));
                stream.Position = 0;
                var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }
    }
}

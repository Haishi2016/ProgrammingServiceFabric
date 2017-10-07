using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using CalculatorService.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using Microsoft.ServiceFabric.Services.Client;

namespace CalculatorWeb.Controllers
{
    [Route("api/[controller]/[action]")]
    public class ValuesController : Controller
    {
        private static ServiceProxyFactory proxyFactory = new ServiceProxyFactory((c) =>
        {
            return new FabricTransportServiceRemotingClientFactory(
                serializationProvider: new ServiceRemotingJsonSerializationProvider(),
                servicePartitionResolver: new ServicePartitionResolver(
                    "localhost:19000",
                    "localhost:19001"
                    )
                );
        });

        [HttpGet]
        public string Add(int a, int b)
        {
            var calculatorClient = proxyFactory.CreateServiceProxy<ICalculatorService>(
                new Uri("fabric:/CalculatorApplication/CalculatorService"),
                targetReplicaSelector: Microsoft.ServiceFabric.Services.Communication.Client.TargetReplicaSelector.RandomInstance);
            return calculatorClient.Add(1, 2).Result;
        }
        
        [HttpGet]
        public string Subtract(int a, int b)
        {
            var calculatorClient = proxyFactory.CreateServiceProxy<ICalculatorService>(
             new Uri("fabric:/CalculatorApplication/CalculatorService"),
             targetReplicaSelector: Microsoft.ServiceFabric.Services.Communication.Client.TargetReplicaSelector.RandomInstance);
            return calculatorClient.Subtract(1, 2).Result;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

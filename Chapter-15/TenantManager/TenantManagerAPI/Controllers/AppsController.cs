using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ApplicationActor.Interfaces;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors;
using System.Threading;

namespace TenantManagerAPI.Controllers
{
    [Route("api/[controller]")]
    public class AppsController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public async Task Post([FromBody]ApplicationState state)
        {
            var proxy = ActorProxy.Create<IApplicationActor>(new ActorId(state.TenantName), new Uri("fabric:/TenantManager/ApplicationActorService"));
            await proxy.CreateTenantAsync(state, default(CancellationToken));
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{tenantName}")]
        public async Task Delete(string tenantName)
        {
            var proxy = ActorProxy.Create<IApplicationActor>(new ActorId(tenantName), new Uri("fabric:/TenantManager/ApplicationActorService"));
            await proxy.DeleteTenantAsync(default(CancellationToken));
        }
    }
}

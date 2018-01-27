using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using SlowProcessor.Interfaces;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors;
using System.Threading;

namespace JobGateway.Controllers
{
    [Route("api/[controller]")]
    public class JobsController : Controller
    {
        IReliableStateManager mStateManager;
        public JobsController(IReliableStateManager stateManager)
        {
            mStateManager = stateManager;
        }
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
        public async Task<string> Post(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new Exception("Value can't be null.");
            var queue = await mStateManager.GetOrAddAsync<IReliableQueue<string>>("JobQueue");
            using (var tx = mStateManager.CreateTransaction())
            {
                var count = await queue.GetCountAsync(tx);
                if (count >= 100)
                {
                    throw new Exception("Service is overwhelmed.");
                }
                else
                {
                    await queue.EnqueueAsync(tx, value);
                    await tx.CommitAsync();
                    return "Active jobs: " + count;
                }
            }
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

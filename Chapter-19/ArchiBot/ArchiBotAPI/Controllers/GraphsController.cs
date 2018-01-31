using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Cors;

namespace ArchiBotAPI.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowSpecificOrigin")]
    public class GraphsController : Controller
    {
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public D3Graph Get(int id)
        {
            D3Graph graph = new D3Graph();
            graph.Nodes.Add(new D3Node { Id = "Myriel", Group = 1 });
            graph.Nodes.Add(new D3Node { Id = "Napoleon", Group = 1 });
            graph.Links.Add(new D3Link { Source = "Napoleon", Target = "Myriel", Value = 1 });

            //return JsonConvert.SerializeObject(graph);
            return graph;
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

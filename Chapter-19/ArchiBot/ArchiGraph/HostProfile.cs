using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiBot.ArchiGraph
{
    public class HostProfile
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("constraints")]
        public IList<Constraint> Constraints { get; set; }
    }
}

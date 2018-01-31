using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiBot.ArchiGraph
{
    public class EntityType
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("id")]
        public string ID { get; set; }
        [JsonProperty("types")]
        public IList<EntityType> Types { get; set; }
        [JsonProperty("options")]
        public IList<Option> Options { get; set; }
        [JsonProperty("features")]
        public Features Features { get; set; }
    }
}

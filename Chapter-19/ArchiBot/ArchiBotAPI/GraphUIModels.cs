using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArchiBotAPI
{
    public class D3Graph
    {
        [JsonProperty("nodes")]
        public List<D3Node> Nodes { get; private set; }
        [JsonProperty("links")]
        public List<D3Link> Links { get; private set; }
        
        public D3Graph()
        {
            Nodes = new List<D3Node>();
            Links = new List<D3Link>();
        }
    }    
    public class D3Node
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("group")]
        public int Group { get; set; }
    }
    public class D3Link
    {
        [JsonProperty("source")]
        public string Source { get; set; }
        [JsonProperty("target")]
        public string Target { get; set; }
        [JsonProperty("value")]
        public int Value { get; set; }
    }
}

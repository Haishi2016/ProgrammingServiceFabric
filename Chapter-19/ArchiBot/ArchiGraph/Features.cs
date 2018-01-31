using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiBot.ArchiGraph
{
    public class Features
    {
        [JsonProperty("horizontal-scale")]
        public int HorizontalScale { get; set; }
        public Features()
        {
            HorizontalScale = 1;
        }
        [JsonProperty("entry-point")]
        public bool EntryPoint { get; set; }
        [JsonProperty("hosts")]
        public HostProfile[] Hosts { get; set; }
        [JsonProperty("proxy")]
        public bool Proxy { get; set; }
    }
}

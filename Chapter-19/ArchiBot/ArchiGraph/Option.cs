using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiBot.ArchiGraph
{
    public class Option
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("values")]
        public IList<string> Values { get; set; }
    }
}

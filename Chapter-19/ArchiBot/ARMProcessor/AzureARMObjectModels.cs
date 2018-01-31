using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiBot.AzureARMObjectModels
{
    public class Template
    {
        [JsonProperty("resources")]
        public IList<Resource> Resources {get;set;}
    }
    public class Resource
    {
        [JsonProperty("apiVersion")]
        public string ApiVersion { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}

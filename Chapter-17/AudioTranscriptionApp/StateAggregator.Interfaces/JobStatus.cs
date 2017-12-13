using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace StateAggregator.Interfaces
{
    [DataContract]
    public class JobStatus
    {
        [DataMember]
        [JsonProperty("name")]
        public string Name { get; set; }
        [DataMember]
        [JsonProperty("url")]
        public string Url { get; set; }
        [DataMember]
        [JsonProperty("percent")]
        public int Percent { get; set; }
        [DataMember]
        [JsonProperty("date")]
        public DateTime Date { get; set; }
        [DataMember]
        [JsonProperty("endDate")]
        public DateTime EndDate { get; set; }
        [DataMember]
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}

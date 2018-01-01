using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationActor.Interfaces
{
    [DataContract]
    public class ApplicationState
    {
        [DataMember]
        public string AppType { get; set; }
        [DataMember]
        public string AppVersion { get; set; }
        [DataMember]
        public string TenantName { get; set;  }
        [DataMember]
        public bool IsRunning { get; set; }
    }
}

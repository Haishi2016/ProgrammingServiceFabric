using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PersonService
{
    [Serializable]
    [DataContract(Name = "Customer", Namespace = "http://www.contoso.com")]
    public class Person
    {
        [DataMember(IsRequired = true)]
        public string FirstName;
        [DataMember]
        public string LastName;
        [DataMember]
        public int ID;
    }
}

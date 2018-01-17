using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ProtoContract]
    public class PostSalesModel
    {
        [ProtoMember(1)]
        public string Product { get; set; }

        [ProtoMember(2)]
        public string Country { get; set; }
    }
}

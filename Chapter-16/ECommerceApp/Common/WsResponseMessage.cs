using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [ProtoContract]
    public class WsResponseMessage
    {
        [ProtoMember(1)]
        public int Result;

        [ProtoMember(2)]
        public byte[] Value;
    }
}

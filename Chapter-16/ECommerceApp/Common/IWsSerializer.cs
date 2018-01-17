using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface IWsSerializer
    {
        Task<byte[]> SerializeAsync<T>(T value);
        Task<T> DeserializeAsync<T>(byte[] serialized);
        Task<T> DeserializeAsync<T>(byte[] serialized, int offset, int length);
    }
}

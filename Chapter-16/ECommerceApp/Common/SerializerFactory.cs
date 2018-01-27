using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class SerializerFactory
    {
        // Set JsonNet as the default serializer
        private static Func<IWsSerializer> DefaultFactory = () => new ProtobufWsSerializer();

        public static void SetFactory<T>() where T : IWsSerializer, new()
        {
            DefaultFactory = () => new T();
        }

        public static IWsSerializer CreateSerializer()
        {
            return DefaultFactory();
        }
    }
}

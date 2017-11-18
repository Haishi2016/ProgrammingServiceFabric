using Microsoft.ServiceFabric.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PersonService
{
    public class PersonSerializer : IStateSerializer<Person>
    {
        public Person Read(BinaryReader binaryReader)
        {
            Person ret = new Person();
            ret.FirstName = binaryReader.ReadString();
            ret.LastName = binaryReader.ReadString();
            ret.ID = binaryReader.ReadInt32();
            return ret;
        }

        public Person Read(Person baseValue, BinaryReader binaryReader)
        {
            return ((IStateSerializer<Person>)this).Read(binaryReader);
        }

        public void Write(Person value, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(value.FirstName);
            binaryWriter.Write(value.LastName);
            binaryWriter.Write(value.ID);
        }

        public void Write(Person baseValue, Person targetValue, BinaryWriter binaryWriter)
        {
            ((IStateSerializer<Person>)this).Write(targetValue, binaryWriter);
        }
    }
}

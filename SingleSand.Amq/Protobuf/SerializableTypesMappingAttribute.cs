using System;
using System.Linq;

namespace SingleSand.Amq.Protobuf
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public class SerializableTypesMappingAttribute : Attribute
    {
        public SerializableTypesMappingAttribute(uint assemblyPrefix, Type mapping)
        {
            if (!mapping.GetInterfaces().Contains(typeof(ISerializableTypesMapping)))
                throw new ArgumentException("Mapping Type should implement ISerializableTypesMapping");
            if (mapping.GetConstructor(Type.EmptyTypes) == null)
                throw new ArgumentException("Mapping Type should have parameterless constructor");
            AssemblyPrefix = assemblyPrefix;
            Mapping = mapping;
        }

        public uint AssemblyPrefix { get; set; }
        public Type Mapping { get; set; }

        internal ISerializableTypesMapping GetMappingInstance()
        {
            var constructor = Mapping.GetConstructor(Type.EmptyTypes);
            return (ISerializableTypesMapping) constructor.Invoke(new object[] {});
        }
    }
}
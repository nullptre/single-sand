using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using ProtoBuf;
using ProtoBuf.Meta;
using SingleSand.Amq.DataModel;

namespace SingleSand.Amq.Protobuf
{
    internal class Formatter : ISerializer
    {
        private const int SyncTimeoutSeconds = 1;
        private static readonly IDictionary<ulong, Type> Mappings = new Dictionary<ulong, Type>();
        private static readonly IDictionary<Type, ulong> ReverseMappings = new Dictionary<Type, ulong>();
        private static readonly ReaderWriterLock SyncRoot = new ReaderWriterLock();

        public static void SetUp()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                LoadSerializableTypes(assembly);
            }
            AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoaded;
        }

        private static void OnAssemblyLoaded(object sender, AssemblyLoadEventArgs args)
        {
            LoadSerializableTypes(args.LoadedAssembly);
        }

        public T Deserialize<T>(byte[] body)
        {
            using (var s = new MemoryStream(body))
            {
                var typeKey = BitConverter.ToUInt64(body, 0);
                s.Seek(sizeof(ulong), SeekOrigin.Begin);
                SyncRoot.AcquireReaderLock(TimeSpan.FromSeconds(SyncTimeoutSeconds));
                Type type;
                try
                {
                    type = Mappings[typeKey];
                }
                finally
                {
                    SyncRoot.ReleaseReaderLock();
                }
                return (T) RuntimeTypeModel.Default.Deserialize(s, null, type);
            }
        }

        public byte[] Serialize<T>(T instance)
        {
            using (var s = new MemoryStream())
            {
                SyncRoot.AcquireReaderLock(TimeSpan.FromSeconds(SyncTimeoutSeconds));
                ulong typeKey;
                try
                {
                    typeKey = ReverseMappings[instance.GetType()];
                }
                finally
                {
                    SyncRoot.ReleaseReaderLock();
                }
                s.Write(BitConverter.GetBytes(typeKey), 0, sizeof(ulong));
                RuntimeTypeModel.Default.Serialize(s, instance);
                return s.ToArray();
            }
        }

        private static void LoadSerializableTypes(Assembly assembly)
        {
            var attributes = assembly.GetCustomAttributes<SerializableTypesMappingAttribute>().ToArray();
            if (attributes.Length == 0)
                return;
            var mappings = attributes
                .Select(a => new { a.AssemblyPrefix, Map = a.GetMappingInstance() })
                .SelectMany(m => m.Map.TypesMapping
                    .Select(tm => new
                        {
                            Prefix = BitConverter.GetBytes((uint) tm.Key)
                                .Concat(BitConverter.GetBytes(m.AssemblyPrefix))
                                .ToArray(),
                            InheritanceTag = tm.Key + short.MaxValue / 2,
                            Type = tm.Value
                        }))
                .Select(tm =>
                    new
                    {
                        Key = BitConverter.ToUInt64(tm.Prefix, 0),
                        Value = tm.Type,
                        tm.InheritanceTag
                    });
            SyncRoot.AcquireWriterLock(TimeSpan.FromSeconds(SyncTimeoutSeconds));
            try
            {
                foreach (var m in mappings)
                {
                    Mappings.Add(m.Key, m.Value);
                    ReverseMappings.Add(m.Value, m.Key);
                    SetUpContractInheritance(m.Value, m.InheritanceTag, RuntimeTypeModel.Default);
                }
            }
            finally
            {
                SyncRoot.ReleaseWriterLock();
            }
        }

        /// <summary>
        /// This method does the same thing as ProtoIncludeAttribute
        /// </summary>
        private static void SetUpContractInheritance(Type type, int inheritanceTag, RuntimeTypeModel model)
        {
            var parent = GetAncestors(type).FirstOrDefault(a => a.GetCustomAttribute<ProtoContractAttribute>() != null);
            if (parent == null)
                return;
            model[parent].AddSubType(inheritanceTag, type);
        }

        private static IEnumerable<Type> GetAncestors(Type type)
        {
            while (type.BaseType != null)
            {
                yield return type.BaseType;
                type = type.BaseType;
            }
        }
    }
}
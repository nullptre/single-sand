using System;
using System.Collections.Generic;
using SingleSand.Amq.Protobuf;

namespace SingleSand.Samples.ModelsTest1
{
    public class ApiCommandsMapping : ISerializableTypesMapping
    {
        public IDictionary<short, Type> TypesMapping
        {
            get
            {
                return new Dictionary<short, Type>
                    {
                        {1, typeof(ApiCommandWithName) }
                    };
            }
        }
    }
}
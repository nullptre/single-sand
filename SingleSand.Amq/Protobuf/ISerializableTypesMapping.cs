using System;
using System.Collections.Generic;

namespace SingleSand.Amq.Protobuf
{
    public interface ISerializableTypesMapping
    {
        IDictionary<short, Type> TypesMapping { get; }
    }
}
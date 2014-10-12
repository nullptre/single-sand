using System;
using System.Collections.Generic;

namespace SingleSand.Samples.Messages
{
    public interface ISerializableTypesMapping
    {
        IDictionary<short, Type> TypesMapping { get; }
    }
}
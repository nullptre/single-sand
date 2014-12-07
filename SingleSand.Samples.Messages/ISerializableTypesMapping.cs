using System;
using System.Collections.Generic;

namespace SingleSand.Samples.Messages
{
    /// <summary>
    /// Each assembly containing serializable classes should include implementation of this interface.
    /// </summary>
    public interface ISerializableTypesMapping
    {
        /// <summary>
        /// Each value defines a serializable type that is konwn for the <see cref="Formatter"/>.
        /// Each key defines unique (within the assembly) identifier of the type.
        /// </summary>
        IDictionary<short, Type> TypesMapping { get; }
    }
}
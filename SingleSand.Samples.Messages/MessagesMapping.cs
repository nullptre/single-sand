using System;
using System.Collections.Generic;

namespace SingleSand.Samples.Messages
{
	internal class MessagesMapping : ISerializableTypesMapping
    {
        public IDictionary<short, Type> TypesMapping
        {
            get
            { 
                return new Dictionary<short, Type>();
            }
        }
    }
}
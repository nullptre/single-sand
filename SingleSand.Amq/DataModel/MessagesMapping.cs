using System;
using System.Collections.Generic;
using SingleSand.Amq.Protobuf;

namespace SingleSand.Amq.DataModel
{
    public class MessagesMapping : ISerializableTypesMapping
    {
        public IDictionary<short, Type> TypesMapping
        {
            get
            { 
                return new Dictionary<short, Type>
                {
                    { 1, typeof(TextMessage) }    
                };
            }
        }
    }
}
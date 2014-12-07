using System;
using System.Collections.Generic;
using SingleSand.Samples.Messages;

namespace SingleSand.Samples.TcpServer.Contracts
{
    internal class ApiCommandsMapping : ISerializableTypesMapping
    {
        public IDictionary<short, Type> TypesMapping
        {
            get
            {
                return new Dictionary<short, Type>
                    {
                        { 1, typeof(ApiCommandWithName) }
                    };
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Cooke.GraphQL.Types;

namespace Cooke.GraphQL
{
    public class Schema
    {
        private readonly IEnumerable<GqlType> _types;

        public Schema(ObjectType queryType, ObjectType mutationType, IEnumerable<GqlType> types)
        {
            _types = types;
            Query = queryType;
            Mutation = mutationType;
        }

        public ObjectType Mutation { get; }

        public ObjectType Query { get; }

        internal GqlType GetType(string value)
        {
            // TODO remove linear search
            return _types.FirstOrDefault(x => x.Name == value);
        }
    }
}
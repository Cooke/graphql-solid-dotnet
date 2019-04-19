using System;
using System.Collections.Generic;
using System.Linq;
using Cooke.GraphQL.Types;

namespace Cooke.GraphQL
{
    public class Schema
    {
        private readonly IEnumerable<GqlType> _types;

        public Schema(GqlObjectType queryType, GqlObjectType mutationType)
        {
            _types = types;
            Query = queryType;
            Mutation = mutationType;
        }

        public GqlObjectType Mutation { get; }

        public GqlObjectType Query { get; }

        internal GqlType GetType(string value)
        {
            // TODO remove linear search
            return _types.FirstOrDefault(x => x.Name == value);
        }
    }
}
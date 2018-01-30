using System.Collections.Generic;
using Cooke.GraphQL.Types;

namespace Cooke.GraphQL
{
    public class Schema
    {
        public Schema(ObjectType queryType, ObjectType mutationType, IEnumerable<BaseType> types)
        {
            Query = queryType;
            Mutation = mutationType;
            Types = types;
        }

        public IEnumerable<BaseType> Types { get; }

        public ObjectType Mutation { get; }

        public ObjectType Query { get; }
    }
}
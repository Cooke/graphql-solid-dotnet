using System.Collections.Generic;
using Cooke.GraphQL.Types;

namespace Cooke.GraphQL
{
    public class Schema
    {
        public Schema(ObjectType queryType, ObjectType mutationType, IEnumerable<TypeDefinition> types)
        {
            Query = queryType;
            Mutation = mutationType;
            Types = types;
        }

        public IEnumerable<TypeDefinition> Types { get; }

        public ObjectType Mutation { get; }

        public ObjectType Query { get; }
    }
}
using System.Collections.Generic;
using Cooke.GraphQL.Types;

namespace Cooke.GraphQL
{
    public class Schema
    {
        public Schema(ObjectGraphType query)
        {
            Query = query;
        }

        public Schema(ObjectGraphType queryType, ObjectGraphType mutationType, IEnumerable<GraphType> types)
        {
            Query = queryType;
            Mutation = mutationType;
            Types = types;
        }

        public IEnumerable<GraphType> Types { get; }

        public ObjectGraphType Mutation { get; }

        public ObjectGraphType Query { get; }
    }
}
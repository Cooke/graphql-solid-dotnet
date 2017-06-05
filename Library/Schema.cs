using System;

namespace Tests
{
    public class Schema
    {
        public Schema(ObjectGraphType query)
        {
            Query = query;
        }

        public Schema(ObjectGraphType queryType, ObjectGraphType mutationType)
        {
            Query = queryType;
            Mutation = mutationType;
        }

        public ObjectGraphType Mutation { get; }

        public ObjectGraphType Query { get; }
    }
}
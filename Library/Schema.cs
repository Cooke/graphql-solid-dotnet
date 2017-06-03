using System;

namespace Tests
{
    public class Schema
    {
        public Schema(ObjectGraphType query)
        {
            Query = query;
        }

        public ObjectGraphType Query { get; }
    }
}
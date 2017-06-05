using System;
using GraphQLParser.AST;

namespace Tests
{
    public abstract class GraphType
    {
        public abstract object CoerceInputValue(GraphQLValue value);
    }
}
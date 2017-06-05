using System;
using GraphQLParser.AST;

namespace Tests
{
    public class GraphQLCoercionException : Exception
    {
        public GraphQLCoercionException(string message, GraphQLLocation location) : base(message)
        {
        }
    }
}
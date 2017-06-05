using System;
using GraphQLParser.AST;

namespace Cooke.GraphQL
{
    public class GraphQLCoercionException : Exception
    {
        public GraphQLCoercionException(string message, GraphQLLocation location) : base(message)
        {
        }
    }
}
using System;
using GraphQLParser.AST;

namespace Cooke.GraphQL
{
    public class TypeCoercionException : Exception
    {
        public TypeCoercionException(string message, GraphQLLocation location) : base(message)
        {
        }
    }
}
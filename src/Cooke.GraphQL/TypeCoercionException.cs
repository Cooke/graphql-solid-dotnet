using System;

namespace Cooke.GraphQL
{
    public class TypeCoercionException : Exception
    {
        public TypeCoercionException(string message) : base(message)
        {
        }
    }
}
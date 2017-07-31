using System;

namespace Cooke.GraphQL
{
    public class FieldErrorException : Exception
    {
        public FieldErrorException(string message) : base(message)
        {
        }
    }
}
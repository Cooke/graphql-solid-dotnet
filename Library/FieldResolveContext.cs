using System;
using System.Collections.Generic;
using Cooke.GraphQL.Types;

namespace Cooke.GraphQL
{
    public class FieldResolveContext
    {
        public FieldResolveContext(object objectValue, Dictionary<string, object> argumentValues, GraphFieldInfo graphFieldInfo)
        {
            Instance = objectValue;
            Args = argumentValues;
            FieldInfo = graphFieldInfo;
        }

        public GraphFieldInfo FieldInfo { get; }

        public object Instance { get; }

        public IDictionary<string, object> Args { get; }
    }

    public class FieldErrorException : Exception
    {
        public FieldErrorException(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}
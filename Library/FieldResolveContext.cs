using System.Collections.Generic;
using Cooke.GraphQL.Types;

namespace Cooke.GraphQL
{
    public class FieldResolveContext
    {
        public FieldResolveContext(object objectValue, Dictionary<string, object> argumentValues, GraphFieldInfo graphFieldInfo)
        {
            Instance = objectValue;
            Arguments = argumentValues;
            FieldInfo = graphFieldInfo;
        }

        public GraphFieldInfo FieldInfo { get; }

        public object Instance { get; }

        public IDictionary<string, object> Arguments { get; }
    }
}
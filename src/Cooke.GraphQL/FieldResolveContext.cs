using System.Collections.Generic;
using Cooke.GraphQL.Types;

namespace Cooke.GraphQL
{
    public class FieldResolveContext<TObject> : FieldResolveContext
    {
        public new TObject Instance => (TObject) base.Instance;

        public FieldResolveContext(TObject objectValue, Dictionary<string, object> argumentValues, GqlFieldInfo graphFieldInfo) : base(objectValue, argumentValues, graphFieldInfo)
        {
        }
    }

    public class FieldResolveContext
    {
        public FieldResolveContext(object objectValue, Dictionary<string, object> argumentValues, GqlFieldInfo graphFieldInfo)
        {
            Instance = objectValue;
            Arguments = argumentValues;
            FieldInfo = graphFieldInfo;
        }

        public GqlFieldInfo FieldInfo { get; }

        public object Instance { get; }

        public IDictionary<string, object> Arguments { get; }
    }
}
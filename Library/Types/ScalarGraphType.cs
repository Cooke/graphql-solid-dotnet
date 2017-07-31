using Cooke.GraphQL.Types;
using Newtonsoft.Json.Linq;

namespace Tests
{
    public abstract class ScalarGraphType : GraphType
    {
        public virtual JValue CoerceResultValue(object resolvedValue)
        {
            return new JValue(resolvedValue);
        }
    }
}
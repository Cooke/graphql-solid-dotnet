using Cooke.GraphQL.Introspection;
using Newtonsoft.Json.Linq;

namespace Cooke.GraphQL.Types
{
    public abstract class ScalarBaseType : TypeDefinition
    {
        public virtual JValue CoerceResultValue(object resolvedValue)
        {
            return new JValue(resolvedValue);
        }

        public override __TypeKind Kind => __TypeKind.Scalar;
    }
}
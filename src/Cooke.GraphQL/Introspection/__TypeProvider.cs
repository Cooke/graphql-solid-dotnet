using System.Collections.Generic;
using Cooke.GraphQL.Types;

namespace Cooke.GraphQL.Introspection
{
    public class __TypeProvider
    {
        private readonly Dictionary<GqlType, __Type> _types = new Dictionary<GqlType, __Type>();

        public __Type GetOrCreateType(GqlType type)
        {
            if (_types.TryGetValue(type, out var returnType))
            {
                return returnType;
            }

            returnType = new __Type(type, this);
            return returnType;
        }

        public void RegisterType(GqlType type, __Type introType)
        {
            _types[type] = introType;
        }
    }
}
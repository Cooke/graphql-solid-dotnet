using System.Collections.Generic;
using Cooke.GraphQL.Types;

namespace Cooke.GraphQL.Introspection
{
    public class __TypeProvider
    {
        private readonly Dictionary<TypeDefinition, __Type> _types = new Dictionary<TypeDefinition, __Type>();

        public __Type GetOrCreateType(TypeDefinition type)
        {
            __Type returnType;
            if (_types.TryGetValue(type, out returnType))
            {
                return returnType;
            }

            returnType = new __Type(type, this);
            return returnType;
        }

        public void RegisterType(TypeDefinition type, __Type introType)
        {
            _types[type] = introType;
        }
    }
}
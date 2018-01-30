using System.Collections.Generic;
using Cooke.GraphQL.Types;

namespace Cooke.GraphQL.Introspection
{
    public class __Field
    {
        public __Field(FieldDescriptor fieldInfo, __TypeProvider typeProvider)
        {
            Name = fieldInfo.Name;
            Type = typeProvider.GetOrCreateType(fieldInfo.Type);
        }

        public __Field(InputFieldDescriptor fieldDescriptor, __TypeProvider typeProvider)
        {
            Name = fieldDescriptor.Name;
            Type = typeProvider.GetOrCreateType(fieldDescriptor.Type);
        }

        public string Name { get; }

        public string Description { get; }

        public IEnumerable<__InputValue> Args { get; } = new List<__InputValue>();

        public bool IsDeprecated => false;

        public string DeprecationReason => "";

        public __Type Type { get; }
    }
}
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

        public __Type Type { get; }
    }
}
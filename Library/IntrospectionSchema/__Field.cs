using Cooke.GraphQL.Types;

namespace Cooke.GraphQL.IntrospectionSchema
{
    public class __Field
    {
        public __Field(GraphFieldInfo fieldInfo, __TypeProvider typeProvider)
        {
            Name = fieldInfo.Name;
            Type = typeProvider.GetOrCreateType(fieldInfo.Type);
        }

        public __Field(GraphInputFieldInfo fieldInfo, __TypeProvider typeProvider)
        {
            Name = fieldInfo.Name;
            Type = typeProvider.GetOrCreateType(fieldInfo.Type);
        }

        public string Name { get; }

        public __Type Type { get; }
    }
}
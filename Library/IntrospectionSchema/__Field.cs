using Cooke.GraphQL.Types;

namespace Cooke.GraphQL.IntrospectionSchema
{
    public class __Field
    {
        public __Field(GraphFieldInfo fieldInfo)
        {
            Name = fieldInfo.Name;
        }

        public __Field(GraphInputFieldInfo fieldInfo)
        {
            Name = fieldInfo.Name;
        }

        public string Name { get; }
    }
}
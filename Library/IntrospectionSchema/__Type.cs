using System.Collections.Generic;
using System.Linq;
using Cooke.GraphQL.Types;

namespace Cooke.GraphQL.IntrospectionSchema
{
    public class __Type
    {
        private readonly GraphType _graphType;

        public __Type(GraphType graphType)
        {
            _graphType = graphType;
            if (_graphType is ObjectGraphType type)
            {
                Fields = type.Fields.Values.Select(x => new __Field(x)).ToArray();
            }
            else if (_graphType is InputObjectGraphType input)
            {
                Fields = input.Fields.Values.Select(x => new __Field(x)).ToArray();
            }
            else if (graphType is EnumGraphType enumType)
            {
                EnumValues = enumType.EnumValues.Select(x => new __EnumValue(x.Value)).ToArray();
            }
        }

        public string Name => _graphType.Name;

        public __TypeKind Kind => _graphType.Kind;

        public IEnumerable<__Field> Fields { get; }

        public IEnumerable<__EnumValue> EnumValues { get; }
    }
}
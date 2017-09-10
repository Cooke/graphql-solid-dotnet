using System.Collections.Generic;
using System.Linq;
using Cooke.GraphQL.Types;

namespace Cooke.GraphQL.Introspection
{
    public class __Type
    {
        private readonly BaseType _graphType;

        public __Type(BaseType graphType, __TypeProvider typeProvider)
        {
            typeProvider.RegisterType(graphType, this);

            _graphType = graphType;

            if (_graphType is ObjectType type)
            {
                Fields = type.Fields.Values.Select(x => new __Field(x, typeProvider)).ToArray();
            }
            else if (_graphType is InputObjectType input)
            {
                Fields = input.Fields.Values.Select(x => new __Field(x, typeProvider)).ToArray();
            }
            else if (graphType is EnumType enumType)
            {
                EnumValues = enumType.EnumValues.Select(x => new __EnumValue(x.Value)).ToArray();
            }
            else if (graphType is ListType listType)
            {
                OfType = typeProvider.GetOrCreateType(listType.ItemType);
            }
            else if (graphType is NonNullType nonNullType)
            {
                OfType = typeProvider.GetOrCreateType(nonNullType.ItemType);
            }
        }

        public string Name => _graphType.Name;

        public __TypeKind Kind => _graphType.Kind;

        public IEnumerable<__Field> Fields { get; }

        public IEnumerable<__EnumValue> EnumValues { get; }

        public __Type OfType { get; }
    }
}
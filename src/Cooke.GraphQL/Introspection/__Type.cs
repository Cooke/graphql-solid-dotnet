using System.Collections.Generic;
using System.Linq;
using Cooke.GraphQL.Types;

namespace Cooke.GraphQL.Introspection
{
    public class __Type
    {
        private readonly GqlType _graphType;

        public __Type(GqlType graphType, __TypeProvider typeProvider)
        {
            typeProvider.RegisterType(graphType, this);

            _graphType = graphType;

            if (_graphType is GqlObjectType type)
            {
                Fields = type.Fields.Values.Select(x => new __Field(x, typeProvider)).ToArray();
                Interfaces = type.Interfaces.Select(typeProvider.GetOrCreateType).ToArray();
            }
            else if (_graphType is InputObjectType input)
            {
                InputFields = input.Fields.Values.Select(x => new __Field(x, typeProvider)).ToArray();
            }
            else if (graphType is GqlEnumType enumType)
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
            else if (graphType is InterfaceType interfaceType)
            {
                Fields = interfaceType.Fields.Values.Select(x => new __Field(x, typeProvider)).ToArray();
                // TODO possible types
            }
        }

        public string Name => _graphType.Name;

        // TODO add support for description
        public string Description => "";

        public __TypeKind Kind => _graphType.Kind;

        public IEnumerable<__Field> Fields { get; } = new List<__Field>();

        public IEnumerable<__Field> InputFields { get; } = new List<__Field>();

        public IEnumerable<__Type> Interfaces { get; } = new List<__Type>();

        public IEnumerable<__Type> PossibleTypes { get; } = new List<__Type>();

        public IEnumerable<__EnumValue> EnumValues { get; } = new List<__EnumValue>();

        public __Type OfType { get; }
    }
}
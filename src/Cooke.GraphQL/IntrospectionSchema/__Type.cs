using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cooke.GraphQL.Types;

namespace Cooke.GraphQL.IntrospectionSchema
{
    public class __Type
    {
        private readonly GraphType _graphType;

        public __Type(GraphType graphType, __TypeProvider typeProvider)
        {
            typeProvider.RegisterType(graphType, this);

            _graphType = graphType;

            if (_graphType is ObjectGraphType type)
            {
                Fields = type.Fields.Values.Select(x => new __Field(x, typeProvider)).ToArray();
            }
            else if (_graphType is InputObjectGraphType input)
            {
                Fields = input.Fields.Values.Select(x => new __Field(x, typeProvider)).ToArray();
            }
            else if (graphType is EnumGraphType enumType)
            {
                EnumValues = enumType.EnumValues.Select(x => new __EnumValue(x.Value)).ToArray();
            }
            else if (graphType is ListGraphType listType)
            {
                OfType = typeProvider.GetOrCreateType(listType.ItemType);
            }
            else if (graphType is NotNullGraphType nonNullType)
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
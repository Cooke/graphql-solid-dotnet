using System;
using System.Collections.Generic;
using System.Reflection;
using Cooke.GraphQL.Annotations;
using Cooke.GraphQL.Introspection;
using GraphQLParser.AST;
using Newtonsoft.Json.Linq;

namespace Cooke.GraphQL.Types
{
    public class EnumValue
    {
        public EnumValue(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }

    class EnumType : ScalarBaseType
    {
        private readonly Type _enumType;

        public EnumType(IEnumerable<EnumValue> enumValues, Type enumType)
        {
            _enumType = enumType;
            EnumValues = enumValues;

            var typeNameAttribute = _enumType.GetTypeInfo().GetCustomAttribute<TypeName>();
            Name = typeNameAttribute?.Name ?? _enumType.Name;
        }

        public IEnumerable<EnumValue> EnumValues { get; }

        public override object CoerceInputValue(GraphQLValue value)
        {
            if (value.Kind != ASTNodeKind.EnumValue)
            {
                throw new TypeCoercionException(
                    $"Input value of type {value.Kind} could not be coerced to int", value.Location);
            }

            var scalarValue = (GraphQLScalarValue) value;
            return Enum.Parse(_enumType, scalarValue.Value, true);
        }

        public override string Name { get; }

        public override __TypeKind Kind => __TypeKind.Enum;

        public override JValue CoerceResultValue(object resolvedValue)
        {
            // TODO add support for custom/non upper enum values
            return new JValue(resolvedValue.ToString().ToUpperInvariant());
        }
    }
}
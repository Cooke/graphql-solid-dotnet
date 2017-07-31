using System;
using System.Collections.Generic;
using GraphQLParser.AST;
using Newtonsoft.Json.Linq;
using Tests;

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

    class EnumGraphType : ScalarGraphType
    {
        private readonly Type _enumType;

        public EnumGraphType(IEnumerable<EnumValue> enumValues, Type enumType)
        {
            _enumType = enumType;
            EnumValues = enumValues;
        }

        public IEnumerable<EnumValue> EnumValues { get; }

        public override object CoerceInputValue(GraphQLValue value)
        {
            if (value.Kind != ASTNodeKind.EnumValue)
            {
                throw new GraphQLCoercionException(
                    $"Input value of type {value.Kind} could not be coerced to int", value.Location);
            }

            var scalarValue = (GraphQLScalarValue) value;
            return Enum.Parse(_enumType, scalarValue.Value, true);
        }

        public override JValue CoerceResultValue(object resolvedValue)
        {
            // TODO add support for custom/non upper enum values
            return new JValue(resolvedValue.ToString().ToUpperInvariant());
        }
    }
}
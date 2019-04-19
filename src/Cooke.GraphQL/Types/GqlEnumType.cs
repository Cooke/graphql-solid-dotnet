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

    class GqlEnumType : GqlType
    {
        private readonly Type _enumType;

        public GqlEnumType(IEnumerable<EnumValue> enumValues, Type enumType)
        {
            _enumType = enumType;
            EnumValues = enumValues;

            var typeNameAttribute = _enumType.GetTypeInfo().GetCustomAttribute<TypeName>();
            Name = typeNameAttribute?.Name ?? _enumType.Name;
        }

        public IEnumerable<EnumValue> EnumValues { get; }

        public override object CoerceInputLiteralValue(GraphQLValue value)
        {
            if (value == null)
            {
                return null;
            }

            if (value.Kind != ASTNodeKind.EnumValue)
            {
                throw new TypeCoercionException(
                    $"Input value of type {value.Kind} could not be coerced to int");
            }

            var scalarValue = (GraphQLScalarValue) value;
            return Enum.Parse(_enumType, scalarValue.Value, true);
        }

        public override object CoerceInputVariableValue(JToken value)
        {
            if (value == null)
            {
                return null;
            }

            if (value.Type != JTokenType.String)
            {
                throw new TypeCoercionException(
                    $"Input variable value of type {value.Type} could not be coerced to an enum value.");
            }

            try
            {
                return Enum.Parse(_enumType, value.Value<string>(), true);
            }
            catch (Exception)
            {
                throw new TypeCoercionException($"Input variable value could not be coerced to a defined enum value.");
            }
        }

        public override string Name { get; }

        public override __TypeKind Kind => __TypeKind.Enum;

        //public override JValue CoerceResultValue(object resolvedValue)
        //{
        //    // TODO add support for custom/non upper enum values
        //    return new JValue(resolvedValue.ToString().ToUpperInvariant());
        //}
    }
}
using System.Collections.Generic;
using GraphQLParser.AST;
using Newtonsoft.Json.Linq;

namespace Cooke.GraphQL.Types
{
    public static class StandardScalars
    {
        public static GqlScalarType Int = new GqlScalarType("Int", CoerceLiteralInt, CoerceInputVariableValue);
        public static GqlScalarType String = new GqlScalarType("String", CoerceLiteralString, CoerceValueString);
        public static GqlScalarType Boolean = new GqlScalarType("Boolean", CoerceBooleanLiteral, CoerceBooleanValue);

        public static IReadOnlyList<GqlScalarType> All = new[]
        {
            Int,
            String,
            Boolean
        };

        private static object CoerceBooleanLiteral(GraphQLValue value)
        {
            if (value == null)
            {
                return null;
            }

            var scalarValue = (GraphQLScalarValue)value;
            if (scalarValue.Value == "true")
            {
                return true;
            }

            if (scalarValue.Value == "false")
            {
                return false;
            }

            throw new TypeCoercionException($"Input value '{scalarValue.Value}' of type {value.Kind} could not be coerced to boolean");
        }

        private static object CoerceBooleanValue(JToken value)
        {
            if (value == null)
            {
                return null;
            }

            if (value.Type != JTokenType.Boolean)
            {
                throw new TypeCoercionException($"Input variable value '{value.ToString()}' of type {value.Type} could not be coerced to boolean.");
            }

            return value.Value<bool>();
        }

        private static object CoerceValueString(JToken value)
        {
            if (value == null)
            {
                return null;
            }

            if (value.Type != JTokenType.String)
            {
                throw new TypeCoercionException(
                    $"Input variable value of type {value.Type} could not be coerced to string.");
            }

            return value.Value<string>();
        }

        private static object CoerceLiteralString(GraphQLValue value)
        {
            if (value == null)
            {
                return null;
            }

            if (value.Kind != ASTNodeKind.StringValue)
            {
                throw new TypeCoercionException(
                    $"Input value of type {value.Kind} could not be coerced to string");
            }

            var stringValue = (GraphQLScalarValue)value;
            return stringValue.Value;
        }

        private static object CoerceLiteralInt(GraphQLValue value)
        {
            if (value == null)
            {
                return null;
            }

            if (value.Kind != ASTNodeKind.IntValue)
            {
                throw new TypeCoercionException(
                    $"Input value of type {value.Kind} could not be coerced to int");
            }

            var scalarValue = (GraphQLScalarValue) value;
            return int.Parse(scalarValue.Value);
        }

        private static object CoerceInputVariableValue(JToken value)
        {
            if (value == null)
            {
                return null;
            }

            if (value.Type != JTokenType.Integer)
            {
                throw new TypeCoercionException("Input variable value is not an integer");
            }

            return value.Value<int>();
        }
    }
}
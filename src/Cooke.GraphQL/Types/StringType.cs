using GraphQLParser.AST;
using Newtonsoft.Json.Linq;

namespace Cooke.GraphQL.Types
{
    public class StringType : ScalarBaseType
    {
        public static StringType Instance { get; } = new StringType();

        private StringType()
        {
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
                    $"Input variable value of type {value.Type} could not be coerced to string.");
            }

            return value.Value<string>();
        }

        public override string Name => "String";

        public override object CoerceInputLiteralValue(GraphQLValue value)
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

            var stringValue = (GraphQLScalarValue) value;
            return stringValue.Value;
        }
    }
}
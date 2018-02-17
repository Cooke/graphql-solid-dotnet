using GraphQLParser.AST;
using Newtonsoft.Json.Linq;

namespace Cooke.GraphQL.Types
{
    public class BooleanType : ScalarBaseType
    {
        public static BooleanType Instance { get; } = new BooleanType();

        private BooleanType()
        {
        }

        public override object CoerceInputLiteralValue(GraphQLValue value)
        {
            if (value == null)
            {
                return null;
            }

            var scalarValue = (GraphQLScalarValue) value;
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

        public override object CoerceInputVariableValue(JToken value)
        {
            if (value == null)
            {
                return null;
            }

            if (value.Type != JTokenType.Boolean)
            {
                throw new TypeCoercionException($"Input variable value '{value.ToString()}' of type {value.Type} could not be coerced to boolean." );
            }

            return value.Value<bool>();
        }

        public override string Name => "Boolean";
    }
}
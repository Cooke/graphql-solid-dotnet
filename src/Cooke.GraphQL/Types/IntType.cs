using GraphQLParser.AST;
using Newtonsoft.Json.Linq;

namespace Cooke.GraphQL.Types
{
    class IntType : ScalarBaseType
    {
        public static IntType Instance { get; } = new IntType();

        private IntType()
        {    
        }

        public override object CoerceInputLiteralValue(GraphQLValue value)
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

        public override object CoerceInputVariableValue(JToken value)
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

        public override string Name => "Int";
    }
}
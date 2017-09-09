using GraphQLParser.AST;

namespace Cooke.GraphQL.Types
{
    public class StringGraphType : ScalarGraphType
    {
        public static StringGraphType Instance { get; } = new StringGraphType();

        private StringGraphType()
        {
        }

        public override string Name => "String";

        public override object CoerceInputValue(GraphQLValue value)
        {
            if (value.Kind != ASTNodeKind.StringValue)
            {
                throw new TypeCoercionException(
                    $"Input value of type {value.Kind} could not be coerced to string", value.Location);
            }

            var stringValue = (GraphQLScalarValue) value;
            return stringValue.Value;
        }
    }
}
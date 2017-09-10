using GraphQLParser.AST;

namespace Cooke.GraphQL.Types
{
    public class StringType : ScalarBaseType
    {
        public static StringType Instance { get; } = new StringType();

        private StringType()
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
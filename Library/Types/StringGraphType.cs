using GraphQLParser.AST;
using Tests;

namespace Cooke.GraphQL.Types
{
    public class StringGraphType : ScalarGraphType
    {
        public override object CoerceInputValue(GraphQLValue value)
        {
            if (value.Kind != ASTNodeKind.StringValue)
            {
                throw new GraphQLCoercionException(
                    $"Input value of type {value.Kind} could not be coerced to string", value.Location);
            }

            var stringValue = (GraphQLScalarValue) value;
            return stringValue.Value;
        }
    }
}
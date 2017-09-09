using GraphQLParser.AST;

namespace Cooke.GraphQL.Types
{
    public class BooleanGraphType : ScalarGraphType
    {
        public static BooleanGraphType Instance { get; } = new BooleanGraphType();

        private BooleanGraphType()
        {
        }

        public override object CoerceInputValue(GraphQLValue value)
        {
            var scalarValue = (GraphQLScalarValue) value;
            if (scalarValue.Value == "true")
            {
                return true;
            }

            if (scalarValue.Value == "false")
            {
                return false;
            }

            throw new TypeCoercionException($"Input value '{scalarValue.Value}' of type {value.Kind} could not be coerced to boolean", value.Location);
        }

        public override string Name => "Boolean";
    }
}
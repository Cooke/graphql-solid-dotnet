using GraphQLParser.AST;

namespace Cooke.GraphQL.Types
{
    class IntType : ScalarBaseType
    {
        public static IntType Instance { get; } = new IntType();

        private IntType()
        {    
        }

        public override object CoerceInputValue(GraphQLValue value)
        {
            if (value.Kind != ASTNodeKind.IntValue)
            {
                throw new TypeCoercionException(
                    $"Input value of type {value.Kind} could not be coerced to int", value.Location);
            }

            var scalarValue = (GraphQLScalarValue) value;
            return int.Parse(scalarValue.Value);
        }

        public override string Name => "Int";
    }
}
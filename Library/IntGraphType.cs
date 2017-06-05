using System;
using GraphQLParser.AST;

namespace Tests
{
    class IntGraphType : ScalarGraphType
    {
        public override object CoerceInputValue(GraphQLValue value)
        {
            if (value.Kind != ASTNodeKind.IntValue)
            {
                throw new GraphQLCoercionException(
                    $"Input value of type {value.Kind} could not be coerced to int", value.Location);
            }

            var scalarValue = (GraphQLScalarValue) value;
            return int.Parse(scalarValue.Value);
        }
    }
}
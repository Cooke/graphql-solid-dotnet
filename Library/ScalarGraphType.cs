using System;
using GraphQLParser.AST;

namespace Tests
{
    public abstract class ScalarGraphType : GraphType
    {
        protected ScalarGraphType(Type clrType) : base(clrType)
        {
        }
    }

    class IntGraphType : ScalarGraphType
    {
        public IntGraphType(Type clrType) : base(clrType)
        {
        }

        public override object InputCoerceValue(GraphQLValue value)
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

    public class StringGraphType : ScalarGraphType
    {
        public StringGraphType(Type clrType) : base(clrType)
        {
        }

        public override object InputCoerceValue(GraphQLValue value)
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

    public class GraphQLCoercionException : Exception
    {
        public GraphQLCoercionException(string message, GraphQLLocation location) : base(message)
        {
        }
    }
}
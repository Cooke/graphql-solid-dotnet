using System;
using Cooke.GraphQL.Introspection;
using GraphQLParser.AST;
using Newtonsoft.Json.Linq;

namespace Cooke.GraphQL.Types
{
    public sealed class NonNullType : TypeDefinition
    {
        public override object CoerceInputLiteralValue(GraphQLValue value)
        {
            var coercedInputValue = ItemType.CoerceInputLiteralValue(value);
            if (coercedInputValue == null)
            {
                throw new TypeCoercionException("A null value cannot be coerced to a non null value.");
            }

            return coercedInputValue;
        }

        public override object CoerceInputVariableValue(JToken value)
        {
            var coerceInputVariableValue = ItemType.CoerceInputVariableValue(value);
            if (coerceInputVariableValue == null)
            {
                throw new TypeCoercionException("A null variable value cannot be coerced to a non null value.");
            }

            return coerceInputVariableValue;
        }

        public TypeDefinition ItemType { get; internal set; }

        public override string Name => null;

        public override __TypeKind Kind => __TypeKind.Non_Null;
    }
}
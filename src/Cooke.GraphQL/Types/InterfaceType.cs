using System;
using Cooke.GraphQL.Introspection;
using GraphQLParser.AST;
using Newtonsoft.Json.Linq;

namespace Cooke.GraphQL.Types
{
    public sealed class InterfaceType : ComplexBaseType
    {
        public InterfaceType(Type clrType) : base(clrType)
        {
        }

        public override __TypeKind Kind => __TypeKind.Interface;

        public override object CoerceInputLiteralValue(GraphQLValue value)
        {
            throw new TypeCoercionException("Cannot coerce an input value to an interface type");
        }

        public override object CoerceInputVariableValue(JToken value)
        {
            throw new TypeCoercionException("Cannot coerce an input variable value to an interface type");
        }
    }
}
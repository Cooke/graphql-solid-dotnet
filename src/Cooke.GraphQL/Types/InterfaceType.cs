using System;
using Cooke.GraphQL.Introspection;
using GraphQLParser.AST;

namespace Cooke.GraphQL.Types
{
    public sealed class InterfaceType : ComplexBaseType
    {
        public InterfaceType(Type clrType) : base(clrType)
        {
        }

        public override __TypeKind Kind => __TypeKind.Interface;

        public override object CoerceInputValue(GraphQLValue value)
        {
            throw new TypeCoercionException("Cannot coerce an input value to an interface type", value.Location);
        }
    }
}
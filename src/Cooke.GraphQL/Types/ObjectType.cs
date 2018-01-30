using System;
using System.Collections.Generic;
using Cooke.GraphQL.Introspection;
using GraphQLParser.AST;

namespace Cooke.GraphQL.Types
{
    public sealed class ObjectType : ComplexBaseType
    {
        public ObjectType(Type clrType) : base(clrType)
        {
        }

        public IEnumerable<InterfaceType> Interfaces { get; set; }

        public override __TypeKind Kind => __TypeKind.Object;

        public override object CoerceInputValue(GraphQLValue value)
        {
            throw new TypeCoercionException("Cannot coerce an input value to an object type", value.Location);
        }
    }
}
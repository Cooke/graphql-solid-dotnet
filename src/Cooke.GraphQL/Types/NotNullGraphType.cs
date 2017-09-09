using System;
using Cooke.GraphQL.Introspection;
using GraphQLParser.AST;

namespace Cooke.GraphQL.Types
{
    public sealed class NotNullGraphType : GraphType
    {
        public override object CoerceInputValue(GraphQLValue value)
        {
            var coercedInputValue = ItemType.CoerceInputValue(value);
            if (coercedInputValue == null)
            {
                // TODO throw something better
                throw new NullReferenceException();
            }

            return coercedInputValue;
        }

        public GraphType ItemType { get; internal set; }

        public override string Name => null;

        public override __TypeKind Kind => __TypeKind.Non_Null;
    }
}
using System.Linq;
using Cooke.GraphQL.Introspection;
using GraphQLParser.AST;

namespace Cooke.GraphQL.Types
{
    public sealed class ListGraphType : GraphType
    {
        public GraphType ItemType { get; internal set; }

        public override object CoerceInputValue(GraphQLValue value)
        {
            if (value is GraphQLListValue listValue)
            {
                return listValue.Values.Select(x => ItemType.CoerceInputValue(x)).ToArray();
            }
            
            return new[] {ItemType.CoerceInputValue(value)};
        }

        public override string Name => null;

        public override __TypeKind Kind => __TypeKind.List;
    }
}
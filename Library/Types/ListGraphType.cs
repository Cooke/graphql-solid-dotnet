using System.Linq;
using Cooke.GraphQL.Types;
using GraphQLParser.AST;

namespace Tests
{
    public class ListGraphType : GraphType
    {
        public ListGraphType(GraphType itemType)
        {
            ItemType = itemType;
        }

        public GraphType ItemType { get; }

        public override object CoerceInputValue(GraphQLValue value)
        {
            if (value is GraphQLListValue listValue)
            {
                return listValue.Values.Select(x => ItemType.CoerceInputValue(x)).ToArray();
            }
            
            return new[] {ItemType.CoerceInputValue(value)};
        }
    }
}
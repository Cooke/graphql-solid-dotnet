using System.Linq;
using Cooke.GraphQL.Types;
using GraphQLParser.AST;

namespace Tests
{
    public class ListGraphType : GraphType
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
    }
}
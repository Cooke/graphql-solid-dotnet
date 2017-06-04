using System;
using System.Linq;
using GraphQLParser.AST;

namespace Tests
{
    public class ListGraphType : GraphType
    {
        public ListGraphType(Type clrType, GraphType itemType) : base(clrType)
        {
            ItemType = itemType;
        }

        public GraphType ItemType { get; }

        public override object InputCoerceValue(GraphQLValue value)
        {
            if (value is GraphQLListValue listValue)
            {
                return listValue.Values.Select(x => ItemType.InputCoerceValue(x)).ToArray();
            }
            
            return new[] {ItemType.InputCoerceValue(value)};
        }
    }
}
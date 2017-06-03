using System;

namespace Tests
{
    public class ListGraphType : GraphType
    {
        public ListGraphType(Type clrType, GraphType itemType) : base(clrType)
        {
            ItemType = itemType;
        }

        public GraphType ItemType { get; }
    }
}
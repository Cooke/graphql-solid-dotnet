using System;

namespace Cooke.GraphQL.Annotations
{
    public class TypeName : Attribute
    {
        public TypeName(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}

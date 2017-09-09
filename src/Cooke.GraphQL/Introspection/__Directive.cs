using System.Collections.Generic;

namespace Cooke.GraphQL.Introspection
{
    public class __Directive
    {
        public string Name { get; }

        public IEnumerable<__DirectiveLocation> Locations { get; }

        public IEnumerable<__InputValue> Args { get; }
    }
}
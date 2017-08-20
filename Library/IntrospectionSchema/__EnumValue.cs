using System.Collections.Generic;

namespace Cooke.GraphQL.IntrospectionSchema
{
    public class __EnumValue
    {
        public __EnumValue(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public bool IsDeprecated { get; }
    }

    public class __Directive
    {
        public string Name { get; }

        public IEnumerable<__DirectiveLocation> Locations { get; }

        public IEnumerable<__InputValue> Args { get; }
    }

    public enum __DirectiveLocation
    {
        Query,
        Mutation,
        Subscription,
        Field,
        FragmentDefinition,
        FragmentSpread,
        InlineFragment,
    }

    public class __InputValue
    {
        public string Name { get; }
        public string Description { get; }
        public __Type Type { get; }
        public string DefaultValue { get; }
    }
}
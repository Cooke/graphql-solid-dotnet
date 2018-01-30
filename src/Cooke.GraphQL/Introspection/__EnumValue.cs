namespace Cooke.GraphQL.Introspection
{
    public class __EnumValue
    {
        public __EnumValue(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public bool IsDeprecated { get; }

        public string Description { get; set; }

        public string DeprecationReason { get; set; }
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
}
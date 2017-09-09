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
namespace Cooke.GraphQL.Types
{
    public class GraphFieldArgumentInfo
    {
        public string Name { get; }

        public bool HasDefaultValue { get; }

        public object DefaultValue { get; }

        public GraphType Type { get; }

        public GraphFieldArgumentInfo(GraphType type, string name, bool hasDefaultValue, object defaultValue)
        {
            Type = type;
            Name = name;
            HasDefaultValue = hasDefaultValue;
            DefaultValue = defaultValue;
        }
    }
}
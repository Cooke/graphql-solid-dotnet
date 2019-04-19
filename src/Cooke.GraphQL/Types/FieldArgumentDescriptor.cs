namespace Cooke.GraphQL.Types
{
    public class FieldArgumentDescriptor
    {
        public string Name { get; }

        public bool HasDefaultValue { get; }

        public object DefaultValue { get; }

        public GqlType Type { get; }

        public FieldArgumentDescriptor(GqlType type, string name, bool hasDefaultValue, object defaultValue)
        {
            Type = type;
            Name = name;
            HasDefaultValue = hasDefaultValue;
            DefaultValue = defaultValue;
        }
    }
}
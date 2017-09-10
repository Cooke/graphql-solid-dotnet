namespace Cooke.GraphQL.Types
{
    public class FieldArgumentDescriptor
    {
        public string Name { get; }

        public bool HasDefaultValue { get; }

        public object DefaultValue { get; }

        public BaseType Type { get; }

        public FieldArgumentDescriptor(BaseType type, string name, bool hasDefaultValue, object defaultValue)
        {
            Type = type;
            Name = name;
            HasDefaultValue = hasDefaultValue;
            DefaultValue = defaultValue;
        }
    }
}
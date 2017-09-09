namespace Cooke.GraphQL.Types
{
    public class EnumValue
    {
        public EnumValue(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }
}
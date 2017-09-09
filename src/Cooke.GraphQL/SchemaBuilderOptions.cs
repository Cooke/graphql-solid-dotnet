namespace Cooke.GraphQL
{
    public class SchemaBuilderOptions
    {
        public IFieldNamingStrategy FieldNamingStrategy { get; set; } = new CamelCaseFieldNamingStrategy();
    }
}
using System;

namespace Tests
{
    public class SchemaTypeBuilderOptions
    {
           
    }

    public class SchemaTypeBuilder
    {
        private Type _queryType;

        public SchemaTypeBuilder UseQuery<T>()
        {
            _queryType = typeof(T);
            return this;
        }

        public SchemaGraphType Build()
        {
            var schemaType = typeof(Schema<>).MakeGenericType(_queryType);
            return ClrToGraphTypeFactory.CreateSchema(schemaType);
        }
    }
}
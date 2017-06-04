using System;

namespace Tests
{
    public class SchemaBuilderOptions
    {
        public IFieldNamingStrategy NamingStrategy { get; set; } = new CamelCaseFieldNamingStrategy();
    }

    public class SchemaBuilder
    {
        private readonly SchemaBuilderOptions _options;
        private Type _queryType;

        public SchemaBuilder(SchemaBuilderOptions options)
        {
            _options = options;
        }

        public SchemaBuilder() : this(new SchemaBuilderOptions())
        {
        }

        public SchemaBuilder UseQuery<T>()
        {
            _queryType = typeof(T);
            return this;
        }

        public Schema Build()
        {
            var factory = new ClrToGraphTypeFactory(_options);
            var graphType = factory.CreateType(_queryType);
            return new Schema((ObjectGraphType) graphType);
        }
    }
}
using System;
using Cooke.GraphQL.Types;

namespace Cooke.GraphQL
{
    public class SchemaBuilderOptions
    {
        public IFieldNamingStrategy NamingStrategy { get; set; } = new CamelCaseFieldNamingStrategy();
    }

    public class SchemaBuilder
    {
        private readonly SchemaBuilderOptions _options;
        private Type _queryType;
        private Type _mutationType;

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

        public SchemaBuilder UseMutation<T>()
        {
            _mutationType = typeof(T);
            return this;
        }

        public Schema Build()
        {
            var factory = new ClrToGraphTypeFactory(_options);
            var graphQueryType = factory.CreateType(_queryType);
            var graphMutationType = _mutationType != null ? factory.CreateType(_mutationType) : null;
            return new Schema((ObjectGraphType) graphQueryType, (ObjectGraphType)graphMutationType);
        }

        
    }
}
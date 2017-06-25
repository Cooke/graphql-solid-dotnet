using System;
using System.Collections.Generic;
using Cooke.GraphQL.Types;
using Tests;

namespace Cooke.GraphQL
{
    public delegate GraphFieldInfo FieldEnhancer(GraphFieldInfo fieldInfo);

    public class SchemaBuilderOptions
    {
        public IFieldNamingStrategy NamingStrategy { get; set; } = new CamelCaseFieldNamingStrategy();
    }

    public class SchemaBuilder
    {
        private readonly SchemaBuilderOptions _options;
        private readonly IList<FieldEnhancer> _fieldEnhancers = new List<FieldEnhancer>();
        private Type _queryType;
        private Type _mutationType;
        private ClrToGraphTypeFactory _factory;

        public SchemaBuilder(SchemaBuilderOptions options)
        {
            _options = options;

            // Default attrubte metadata enhancer
            this.UseAttributeMetadata();
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

        public SchemaBuilder UseFieldEnhancer(FieldEnhancer fieldEnhancer)
        {
            _fieldEnhancers.Add(fieldEnhancer);
            return this;
        }

        public Schema Build()
        {
            _factory = new ClrToGraphTypeFactory(_options, _fieldEnhancers);
            var graphQueryType = _factory.CreateType(_queryType);
            var graphMutationType = _mutationType != null ? _factory.CreateType(_mutationType) : null;
            return new Schema((ObjectGraphType) graphQueryType, (ObjectGraphType)graphMutationType);
        }
    }
}
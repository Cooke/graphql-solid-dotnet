using System.Collections.Generic;
using System.Linq;
using Cooke.GraphQL.Types;

namespace Cooke.GraphQL.Introspection
{
    internal class __Schema
    {
        public __Schema(Schema schema, Schema introspectionSchema)
        {
            var typeProvider = new __TypeProvider();
            // The introspection query type is an implementation detail and should not be visible to the consumer
            var visibleIntrospectionTypes = introspectionSchema.Types.Where(x => !(x is ObjectGraphType o && o.ClrType == typeof(IntrospectionQuery)));
            Types = schema.Types.Concat(visibleIntrospectionTypes).Distinct().Where(x => x.Name != null).Select(x => typeProvider.GetOrCreateType(x)).ToArray();
            Directives = new List<__Directive>();
            QueryType = typeProvider.GetOrCreateType(schema.Query);
        }

        public IReadOnlyCollection<__Type> Types { get; }
        
        public IReadOnlyCollection<__Directive> Directives { get; }

        public __Type QueryType { get; }
    }
}
using System.Collections.Generic;
using System.Linq;
using Cooke.GraphQL.Types;

namespace Cooke.GraphQL.IntrospectionSchema
{
    internal class __Schema
    {
        public __Schema(Schema schema, Schema introspectionSchema)
        {
            // The introspection query type is an implementation detail and should not be visible to the consumer
            var visibleIntrospectionTypes = introspectionSchema.Types.Where(x => !(x is ObjectGraphType o && o.ClrType == typeof(IntrospectionQuery)));
            Types = schema.Types.Concat(visibleIntrospectionTypes).Distinct().Where(x => x.Name != null).Select(x => new __Type(x)).ToArray();
            Directives = new List<__Directive>();
            QueryType = new __Type(schema.Query);
        }

        public IReadOnlyCollection<__Type> Types { get; }
        
        public IReadOnlyCollection<__Directive> Directives { get; }

        public __Type QueryType { get; }
    }
}
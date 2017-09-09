using System.Linq;
// ReSharper disable InconsistentNaming

namespace Cooke.GraphQL.Introspection
{
    internal class IntrospectionQuery
    {
        public IntrospectionQuery(Schema schema, Schema introspectionSchema)
        {
            __schema = new __Schema(schema, introspectionSchema);
        }

        public __Schema __schema { get; }

        public __Type __type(string name)
        {
            return __schema.Types.FirstOrDefault(x => x.Name == name);
        }
    }
}

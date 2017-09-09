using System;

namespace Cooke.GraphQL
{
    public static class QueryExecutorBuilderExtentions
    {
        public static QueryExecutorBuilder WithResolver(this QueryExecutorBuilder builder, IServiceProvider sp)
        {
            return builder.WithResolver(sp.GetService);
        }
    }
}

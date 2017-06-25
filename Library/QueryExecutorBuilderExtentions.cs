using System;
using System.Collections.Generic;
using System.Text;

namespace Cooke.GraphQL.AspNetCore
{
    public static class QueryExecutorBuilderExtentions
    {
        public static QueryExecutorBuilder UseResolver(this QueryExecutorBuilder builder, IServiceProvider sp)
        {
            return builder.WithResolver(sp.GetService);
        }
    }
}

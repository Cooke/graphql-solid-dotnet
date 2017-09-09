using System;
using System.Collections.Generic;

namespace Cooke.GraphQL
{
    public class QueryExecutorOptions
    {
        public Func<Type, object> Resolver { get; set; } = t => Activator.CreateInstance(t);

        public IList<Type> MiddlewareTypes { get; set; } = new List<Type>();
    }
}
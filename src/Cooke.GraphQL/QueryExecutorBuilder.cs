using System;
using System.Collections.Generic;

namespace Cooke.GraphQL
{
    public class QueryExecutorBuilder
    {
        private static readonly QueryExecutorOptions DefaultQueryExecutorOptions = new QueryExecutorOptions();

        private readonly IList<Type> _middlewares = new List<Type>();
        private Schema _schema;
        private Func<Type, object> _resolver;

        public QueryExecutorBuilder WithSchema(Schema schema)
        {
            _schema = schema;
            return this;
        }

        public QueryExecutorBuilder UseMiddleware<T>()
        {
            _middlewares.Add(typeof(T));
            return this;
        }

        public QueryExecutorBuilder WithResolver(Func<Type, object> resolver)
        {
            _resolver = resolver;
            return this;
        }

        public QueryExecutor Build()
        {
            return new QueryExecutor(_schema, new QueryExecutorOptions
            {
                Resolver = _resolver ?? DefaultQueryExecutorOptions.Resolver,
                MiddlewareTypes = _middlewares
            });
        }
    }
}
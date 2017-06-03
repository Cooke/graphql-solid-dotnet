using System;

namespace Tests
{
    public class Schema<TQuery>
    {
        private readonly Func<Type, object> _resolver;

        public Schema(Func<Type, object> resolver)
        {
            _resolver = resolver;
        }

        public TQuery Query => (TQuery)_resolver(typeof(TQuery));
    }
}
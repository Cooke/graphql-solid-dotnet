using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cooke.GraphQL.Types
{
    public class GraphFieldInfo
    {
        private readonly Func<object, IDictionary<string, object>, Task<object>> _resolver;

        public GraphFieldInfo(string name, GraphType type, Func<object, IDictionary<string, object>, Task<object>> resolver, GraphFieldArgumentInfo[] arguments)
        {
            _resolver = resolver;
            Name = name;
            Type = type;
            Arguments = arguments;
        }

        public string Name { get; }

        public GraphType Type { get; }

        public GraphFieldArgumentInfo[] Arguments { get; }

        public Task<object> ResolveAsync(object objectValue, Dictionary<string, object> arguments)
        {
            return _resolver(objectValue, arguments);
        }
    }
}
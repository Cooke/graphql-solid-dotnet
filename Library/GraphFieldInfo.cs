using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tests
{
    public class GraphFieldInfo
    {
        private readonly Func<object, IDictionary<string, object>, Task<object>> _resolver;

        public GraphFieldInfo(string name, GraphType type, Func<object, IDictionary<string, object>, Task<object>> resolver, FieldArgumentInfo[] arguments)
        {
            _resolver = resolver;
            Name = name;
            Type = type;
            Arguments = arguments;
        }

        public string Name { get; }

        public GraphType Type { get; }

        public FieldArgumentInfo[] Arguments { get; }

        public Task<object> ResolveAsync(object objectValue, Dictionary<string, object> arguments)
        {
            return _resolver(objectValue, arguments);
        }
    }

    public class GraphInputFieldInfo
    {
        private readonly Action<object, object> _setter;

        public GraphInputFieldInfo(string name, GraphType type, Action<object, object> setter)
        {
            _setter = setter;
            Name = name;
            Type = type;
        }

        public string Name { get; }

        public GraphType Type { get; }

        public void Set(object instance, object value)
        {
            _setter(instance, value);
        }
    }
}
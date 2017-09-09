using System;

namespace Cooke.GraphQL.Types
{
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
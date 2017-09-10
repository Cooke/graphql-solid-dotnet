using System;

namespace Cooke.GraphQL.Types
{
    public class InputFieldDescriptor
    {
        private readonly Action<object, object> _setter;

        public InputFieldDescriptor(string name, BaseType type, Action<object, object> setter)
        {
            _setter = setter;
            Name = name;
            Type = type;
        }

        public string Name { get; }

        public BaseType Type { get; }

        public void Set(object instance, object value)
        {
            _setter(instance, value);
        }
    }
}
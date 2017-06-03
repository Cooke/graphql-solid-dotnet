using System;
using System.Threading.Tasks;

namespace Tests
{
    public class GraphFieldInfo
    {
        private readonly Func<object, Task<object>> _resolver;

        public GraphFieldInfo(string name, GraphType type, Func<object, Task<object>> resolver)
        {
            _resolver = resolver;
            Name = name;
            Type = type;
        }

        public string Name { get; }
        public GraphType Type { get; }

        public Task<object> ResolveAsync(object objectValue)
        {
            return _resolver(objectValue);
        }
    }
}
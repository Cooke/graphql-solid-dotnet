using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tests
{
    public class ObjectGraphType : GraphType
    {
        private readonly Dictionary<string, GraphFieldInfo> _fields;

        public ObjectGraphType(Type clrType, Dictionary<string, GraphFieldInfo> fields) : base(clrType)
        {
            _fields = fields;
        }

        public async Task<object> ResolveAsync(object objectValue, string fieldName)
        {
            return await _fields[fieldName].ResolveAsync(objectValue);
        }

        public GraphType GetFieldType(string fieldName)
        {
            return _fields[fieldName].Type;
        }
    }
}
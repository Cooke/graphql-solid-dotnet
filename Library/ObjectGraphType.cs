using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQLParser.AST;

namespace Tests
{
    public class ObjectGraphType : GraphType
    {
        private readonly Dictionary<string, GraphFieldInfo> _fields;

        public ObjectGraphType(Type clrType, Dictionary<string, GraphFieldInfo> fields) : base(clrType)
        {
            _fields = fields;
        }

        public async Task<object> ResolveAsync(object objectValue, string fieldName, Dictionary<string, object> argumentValues)
        {
            return await _fields[fieldName].ResolveAsync(objectValue, argumentValues);
        }

        public GraphType GetFieldType(string fieldName)
        {
            return _fields[fieldName].Type;
        }

        public FieldArgumentInfo[] GetArgumentDefinitions(string fieldName)
        {
            return _fields[fieldName].Arguments;
        }

        public override object InputCoerceValue(GraphQLValue value)
        {
            throw new GraphQLCoercionException("Cannot coerce an input value to an object type", value.Location);
        }
    }
}
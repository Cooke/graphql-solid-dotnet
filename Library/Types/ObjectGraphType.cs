using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQLParser.AST;

namespace Cooke.GraphQL.Types
{
    public class ObjectGraphType : GraphType
    {
        private readonly Dictionary<string, GraphFieldInfo> _fields;

        public ObjectGraphType(Type clrType, Dictionary<string, GraphFieldInfo> fields)
        {
            ClrType = clrType;
            _fields = fields;
        }

        public Type ClrType { get; }

        public async Task<object> ResolveAsync(object objectValue, string fieldName, Dictionary<string, object> argumentValues)
        {
            return await _fields[fieldName].ResolveAsync(objectValue, argumentValues);
        }

        public GraphType GetFieldType(string fieldName)
        {
            return _fields[fieldName].Type;
        }

        public GraphFieldArgumentInfo[] GetArgumentDefinitions(string fieldName)
        {
            return _fields[fieldName].Arguments;
        }

        public override object CoerceInputValue(GraphQLValue value)
        {
            throw new GraphQLCoercionException("Cannot coerce an input value to an object type", value.Location);
        }
    }

    public class InputObjectGraphType : GraphType
    {
        private readonly Dictionary<string, GraphInputFieldInfo> _fields;

        public InputObjectGraphType(Type clrType, Dictionary<string, GraphInputFieldInfo> fields)
        {
            ClrType = clrType;
            _fields = fields;
        }

        public Type ClrType { get; }

        public GraphType GetFieldType(string fieldName)
        {
            return _fields[fieldName].Type;
        }

        public override object CoerceInputValue(GraphQLValue value)
        {
            if (!(value is GraphQLObjectValue objectValue))
            {
                throw new GraphQLCoercionException("Cannot coerce the given input kind to an input object type", value.Location);
            }

            var instance = Activator.CreateInstance(ClrType);
            foreach (var inputField in objectValue.Fields)
            {
                if (!_fields.ContainsKey(inputField.Name.Value))
                {
                    throw new GraphQLCoercionException("The given field is not valid", inputField.Name.Location);
                }

                var fieldInfo = _fields[inputField.Name.Value];
                fieldInfo.Set(instance, fieldInfo.Type.CoerceInputValue(inputField.Value));
            }

            // TODO make sure all non-null field have been set

            return instance;
        }
    }
}
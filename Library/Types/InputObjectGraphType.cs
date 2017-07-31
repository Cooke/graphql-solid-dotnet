using System;
using System.Collections.Generic;
using GraphQLParser.AST;

namespace Cooke.GraphQL.Types
{
    public sealed class InputObjectGraphType : GraphType
    {
        public InputObjectGraphType(Type clrType)
        {
            ClrType = clrType;
        }

        public Dictionary<string, GraphInputFieldInfo> Fields { get; internal set; }

        public Type ClrType { get; }

        public GraphType GetFieldType(string fieldName)
        {
            return Fields[fieldName].Type;
        }

        public override object CoerceInputValue(GraphQLValue value)
        {
            if (!(value is GraphQLObjectValue objectValue))
            {
                throw new TypeCoercionException("Cannot coerce the given input kind to an input object type", value.Location);
            }

            var instance = Activator.CreateInstance(ClrType);
            foreach (var inputField in objectValue.Fields)
            {
                if (!Fields.ContainsKey(inputField.Name.Value))
                {
                    throw new TypeCoercionException("The given field is not valid", inputField.Name.Location);
                }

                var fieldInfo = Fields[inputField.Name.Value];
                fieldInfo.Set(instance, fieldInfo.Type.CoerceInputValue(inputField.Value));
            }

            // TODO make sure all non-null field have been set

            return instance;
        }
    }
}
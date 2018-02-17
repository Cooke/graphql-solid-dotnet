using System;
using System.Collections.Generic;
using System.Reflection;
using Cooke.GraphQL.Annotations;
using Cooke.GraphQL.Introspection;
using GraphQLParser.AST;
using Newtonsoft.Json.Linq;

namespace Cooke.GraphQL.Types
{
    public sealed class InputObjectType : BaseType
    {
        public InputObjectType(Type clrType)
        {
            ClrType = clrType;
            var typeNameAttribute = clrType.GetTypeInfo().GetCustomAttribute<TypeName>();
            Name = typeNameAttribute?.Name ?? clrType.Name;
        }

        public Dictionary<string, InputFieldDescriptor> Fields { get; internal set; }

        public Type ClrType { get; }

        public BaseType GetFieldType(string fieldName)
        {
            return Fields[fieldName].Type;
        }

        public override object CoerceInputLiteralValue(GraphQLValue value)
        {
            if (value == null)
            {
                return null;
            }

            if (!(value is GraphQLObjectValue objectValue))
            {
                throw new TypeCoercionException("Cannot coerce the given input kind to an input object type");
            }

            var instance = Activator.CreateInstance(ClrType);
            foreach (var inputField in objectValue.Fields)
            {
                if (!Fields.ContainsKey(inputField.Name.Value))
                {
                    throw new TypeCoercionException("The given field is not valid");
                }

                var fieldInfo = Fields[inputField.Name.Value];
                fieldInfo.Set(instance, fieldInfo.Type.CoerceInputLiteralValue(inputField.Value));
            }

            // TODO make sure all non-null field have been set

            return instance;
        }

        public override object CoerceInputVariableValue(JToken value)
        {
            if (value == null)
            {
                return null;
            }

            if (value.Type != JTokenType.Object)
            {
                throw new TypeCoercionException($"Cannot coerce the input variable value of type {value.Type} to an input object type.");
            }

            // TODO make sure all non-null field have been set

            return value.ToObject(ClrType);
        }

        public override string Name { get; }

        public override __TypeKind Kind => __TypeKind.InputObject;
    }
}
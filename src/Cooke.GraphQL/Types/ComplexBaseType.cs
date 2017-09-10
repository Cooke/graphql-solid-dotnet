using System;
using System.Collections.Generic;
using System.Reflection;
using Cooke.GraphQL.Annotations;

namespace Cooke.GraphQL.Types
{
    public abstract class ComplexBaseType : BaseType
    {
        protected ComplexBaseType(Type clrType)
        {
            ClrType = clrType;
            var typeNameAttribute = clrType.GetTypeInfo().GetCustomAttribute<TypeName>();
            Name = typeNameAttribute?.Name ?? clrType.Name;
        }

        public Dictionary<string, FieldDescriptor> Fields { get; internal set; }

        internal Type ClrType { get; }

        public override string Name { get; }

        public FieldDescriptor GetFieldInfo(string fieldName)
        {
            return Fields[fieldName];
        }

        public BaseType GetFieldType(string fieldName)
        {
            return Fields[fieldName].Type;
        }

        public FieldArgumentDescriptor[] GetArgumentDefinitions(string fieldName)
        {
            return Fields[fieldName].Arguments;
        }
    }
}
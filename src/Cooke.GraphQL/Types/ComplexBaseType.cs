using System;
using System.Collections.Generic;
using System.Reflection;
using Cooke.GraphQL.Annotations;

namespace Cooke.GraphQL.Types
{
    public abstract class ComplexBaseType : TypeDefinition
    {
        protected ComplexBaseType(Type clrType)
        {
            ClrType = clrType;
            var typeNameAttribute = clrType.GetTypeInfo().GetCustomAttribute<TypeName>();
            Name = typeNameAttribute?.Name ?? clrType.Name;
        }

        public Dictionary<string, FieldDefinition> Fields { get; internal set; }

        internal Type ClrType { get; }

        public override string Name { get; }

        public FieldDefinition GetFieldInfo(string fieldName)
        {
            return Fields[fieldName];
        }

        public TypeDefinition GetFieldType(string fieldName)
        {
            return Fields[fieldName].Type;
        }

        public FieldArgumentDescriptor[] GetArgumentDefinitions(string fieldName)
        {
            return Fields[fieldName].Arguments;
        }
    }
}
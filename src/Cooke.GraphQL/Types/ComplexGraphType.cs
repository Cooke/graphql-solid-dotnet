using System;
using System.Collections.Generic;
using System.Reflection;
using Cooke.GraphQL.Annotations;

namespace Cooke.GraphQL.Types
{
    public abstract class ComplexGraphType : GraphType
    {
        protected ComplexGraphType(Type clrType)
        {
            ClrType = clrType;
            var typeNameAttribute = clrType.GetTypeInfo().GetCustomAttribute<TypeName>();
            Name = typeNameAttribute?.Name ?? clrType.Name;
        }

        public Dictionary<string, GraphFieldInfo> Fields { get; internal set; }

        internal Type ClrType { get; }

        public override string Name { get; }

        public GraphFieldInfo GetFieldInfo(string fieldName)
        {
            return Fields[fieldName];
        }

        public GraphType GetFieldType(string fieldName)
        {
            return Fields[fieldName].Type;
        }

        public GraphFieldArgumentInfo[] GetArgumentDefinitions(string fieldName)
        {
            return Fields[fieldName].Arguments;
        }
    }
}
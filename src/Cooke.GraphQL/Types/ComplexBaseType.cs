using System;
using System.Collections.Generic;
using System.Reflection;
using Cooke.GraphQL.Annotations;

namespace Cooke.GraphQL.Types
{
    public abstract class ComplexBaseType : GqlType
    {
        private readonly Lazy<Dictionary<string, GqlFieldInfo>> _fields;

        protected ComplexBaseType(string name, Func<Dictionary<string, GqlFieldInfo>> fieldProvider)
        {
            Name = name;
            _fields = new Lazy<Dictionary<string, GqlFieldInfo>>(fieldProvider);
        }

        public Dictionary<string, GqlFieldInfo> Fields => _fields.Value;

        public override string Name { get; }

        public GqlFieldInfo GetFieldInfo(string fieldName)
        {
            return Fields[fieldName];
        }

        public GqlType GetFieldType(string fieldName)
        {
            return Fields[fieldName].Type;
        }

        public FieldArgumentDescriptor[] GetArgumentDefinitions(string fieldName)
        {
            return Fields[fieldName].Arguments;
        }
    }
}
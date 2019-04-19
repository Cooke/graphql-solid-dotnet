using System;
using System.Collections.Generic;
using System.Reflection;
using Cooke.GraphQL.Annotations;

namespace Cooke.GraphQL.Types
{
    public abstract class ComplexBaseType : GqlType
    {
        protected ComplexBaseType(string name, Dictionary<string, GqlFieldInfo> fields)
        {
            Name = name;
            Fields = fields;
        }

        public Dictionary<string, GqlFieldInfo> Fields { get; }

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
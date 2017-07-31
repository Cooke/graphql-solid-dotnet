using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQLParser.AST;

namespace Cooke.GraphQL.Types
{
    public sealed class ObjectGraphType : GraphType
    {
        public ObjectGraphType(Type clrType)
        {
            ClrType = clrType;
        }

        public Dictionary<string, GraphFieldInfo> Fields { get; internal set; }

        internal Type ClrType { get; }
    
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

        public override object CoerceInputValue(GraphQLValue value)
        {
            throw new TypeCoercionException("Cannot coerce an input value to an object type", value.Location);
        }
    }
}
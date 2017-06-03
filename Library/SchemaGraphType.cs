using System;
using System.Collections.Generic;

namespace Tests
{
    public class SchemaGraphType : ObjectGraphType
    {
        private readonly Type _clrType;

        public SchemaGraphType(Type clrType, Dictionary<string, GraphFieldInfo> fields) : base(clrType, fields)
        {
            _clrType = clrType;
        }

        public object Create()
        {
            return Create(Activator.CreateInstance);
        }

        public object Create(Func<Type, object> resolver)
        {
            return Activator.CreateInstance(_clrType, resolver);
        }
    }
}
using System;
using System.Collections.Generic;
using Cooke.GraphQL.Introspection;
using GraphQLParser.AST;
using Newtonsoft.Json.Linq;

namespace Cooke.GraphQL.Types
{
    public sealed class ObjectType : ComplexBaseType
    {
        public ObjectType(string name) : base(name)
        {
        }

        public IEnumerable<InterfaceType> Interfaces { get; }

        public override __TypeKind Kind => __TypeKind.Object;

        public override object CoerceInputLiteralValue(GraphQLValue value)
        {
            throw new TypeCoercionException("Cannot coerce an input value to an object type");
        }

        public override object CoerceInputVariableValue(JToken value)
        {
            throw new TypeCoercionException("Cannot coerce an input variable value to an object type");
        }
    }
}
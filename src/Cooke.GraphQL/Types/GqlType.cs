using System;
using Cooke.GraphQL.Introspection;
using GraphQLParser.AST;
using Newtonsoft.Json.Linq;

namespace Cooke.GraphQL.Types
{
    public abstract class GqlType
    {
        public abstract object CoerceInputLiteralValue(GraphQLValue value);

        public abstract object CoerceInputVariableValue(JToken value);

        public abstract string Name { get; }

        public abstract __TypeKind Kind { get; }

        public Type ClrType { get; set; }
    }
}
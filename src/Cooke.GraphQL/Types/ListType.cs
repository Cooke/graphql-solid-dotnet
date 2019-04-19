using System.Linq;
using Cooke.GraphQL.Introspection;
using GraphQLParser.AST;
using Newtonsoft.Json.Linq;

namespace Cooke.GraphQL.Types
{
    public sealed class ListType : TypeDefinition
    {
        public TypeDefinition ItemType { get; internal set; }

        public override object CoerceInputLiteralValue(GraphQLValue value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is GraphQLListValue listValue)
            {
                return listValue.Values.Select(x => ItemType.CoerceInputLiteralValue(x)).ToArray();
            }
            
            return new[] {ItemType.CoerceInputLiteralValue(value)};
        }

        public override object CoerceInputVariableValue(JToken value)
        {
            if (value == null)
            {
                return null;
            }

            if (value.Type == JTokenType.Array)
            {
                return value.AsJEnumerable().Select(x => ItemType.CoerceInputVariableValue(x)).ToArray();
            }

            return new[] {ItemType.CoerceInputVariableValue(value)};
        }

        public override string Name => null;

        public override __TypeKind Kind => __TypeKind.List;
    }
}
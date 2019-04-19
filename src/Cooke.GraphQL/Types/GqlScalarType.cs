using System;
using Cooke.GraphQL.Introspection;
using GraphQLParser.AST;
using Newtonsoft.Json.Linq;

namespace Cooke.GraphQL.Types
{
    public class GqlScalarType : GqlType
    {
        private readonly Func<GraphQLValue, object> _literalInputCoercer;
        private readonly Func<JToken, object> _valueInputCoercer;

        public GqlScalarType(string name, Func<GraphQLValue, object> literalInputCoercer, Func<JToken, object> valueInputCoercer)
        {
            _literalInputCoercer = literalInputCoercer;
            _valueInputCoercer = valueInputCoercer;
            Name = name;
        }

        public override string Name { get; }

        public override __TypeKind Kind => __TypeKind.Scalar;

        public virtual JValue CoerceResultValue(object resolvedValue)
        {
            return new JValue(resolvedValue);
        }

        public override object CoerceInputLiteralValue(GraphQLValue value)
        {
            return _literalInputCoercer(value);
        }

        public override object CoerceInputVariableValue(JToken value)
        {
            return _valueInputCoercer(value);
        }
    }
}
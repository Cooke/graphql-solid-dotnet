using Cooke.GraphQL.IntrospectionSchema;
using GraphQLParser.AST;

namespace Cooke.GraphQL.Types
{
    public abstract class GraphType
    {
        public abstract object CoerceInputValue(GraphQLValue value);

        public abstract string Name { get; }

        public abstract __TypeKind Kind { get; }
    }
}
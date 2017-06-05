using GraphQLParser.AST;

namespace Cooke.GraphQL.Types
{
    public abstract class GraphType
    {
        public abstract object CoerceInputValue(GraphQLValue value);
    }
}
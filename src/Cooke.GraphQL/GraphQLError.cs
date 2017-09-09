namespace Cooke.GraphQL
{
    public class GraphQLError
    {
        public string Message { get; }

        public GraphQLError(string message)
        {
            Message = message;
        }
    }
}
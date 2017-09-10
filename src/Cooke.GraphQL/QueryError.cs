namespace Cooke.GraphQL
{
    public class QueryError
    {
        public string Message { get; }

        public QueryError(string message)
        {
            Message = message;
        }
    }
}
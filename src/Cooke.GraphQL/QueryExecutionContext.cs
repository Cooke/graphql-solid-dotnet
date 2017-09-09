using System.Collections.Generic;

namespace Cooke.GraphQL
{
    public class QueryExecutionContext
    {
        private readonly IList<GraphQLError> _errors = new List<GraphQLError>();

        public void AddError(GraphQLError error)
        {
            _errors.Add(error);
        }

        public IEnumerable<GraphQLError> Errors => _errors;
    }
}
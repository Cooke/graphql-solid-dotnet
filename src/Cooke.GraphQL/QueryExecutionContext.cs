using System.Collections.Generic;

namespace Cooke.GraphQL
{
    public class QueryExecutionContext
    {
        private readonly IList<QueryError> _errors = new List<QueryError>();

        public void AddError(QueryError error)
        {
            _errors.Add(error);
        }

        public IEnumerable<QueryError> Errors => _errors;
    }
}
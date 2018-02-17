using System.Collections.Generic;
using System.Collections.ObjectModel;
using GraphQLParser.AST;
using Newtonsoft.Json.Linq;

namespace Cooke.GraphQL
{
    public class QueryExecutionContext
    {
        private readonly IList<QueryError> _errors = new List<QueryError>();

        public QueryExecutionContext(Dictionary<string, GraphQLFragmentDefinition> fragmentDefinitions, Dictionary<string, object> variables)
        {
            Variables = new ReadOnlyDictionary<string, object>(variables);
            FragmentDefinitions = new ReadOnlyDictionary<string, GraphQLFragmentDefinition>(fragmentDefinitions);
        }

        public void AddError(QueryError error)
        {
            _errors.Add(error);
        }

        public IEnumerable<QueryError> Errors => _errors;

        public ReadOnlyDictionary<string, GraphQLFragmentDefinition> FragmentDefinitions { get; }

        public ReadOnlyDictionary<string, object> Variables { get; }
    }
}
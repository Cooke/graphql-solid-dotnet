using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cooke.GraphQL.Types;
using GraphQLParser;
using GraphQLParser.AST;
using Newtonsoft.Json.Linq;
using Tests;

namespace Cooke.GraphQL
{
    public class QueryExecutorOptions
    {
        public Func<Type, object> Resolver = t => Activator.CreateInstance(t);

        public IList<Type> MiddlewareTypes { get; set; } = new List<Type>();
    }

    public class QueryExecutorBuilder
    {
        private Schema _schema;
        private IList<Type> _middlewares = new List<Type>();
        private Func<Type, object> _resolver;

        public QueryExecutorBuilder WithSchema(Schema schema)
        {
            _schema = schema;
            return this;
        }

        public QueryExecutorBuilder UseMiddleware<T>()
        {
            _middlewares.Add(typeof(T));
            return this;
        }

        public QueryExecutorBuilder WithResolver(Func<Type, object> resolver)
        {
            _resolver = resolver;
            return this;
        }

        public QueryExecutor Build()
        {
            return new QueryExecutor(_schema, new QueryExecutorOptions
            {
                Resolver = _resolver,
                MiddlewareTypes = _middlewares
            });
        }
    }


    public class QueryExecutionContext
    {
        private readonly IList<GraphError> _errors = new List<GraphError>();

        public void AddError(GraphError error)
        {
            _errors.Add(error);
        }

        public IEnumerable<GraphError> Errors => _errors;
    }

    public class GraphError
    {
        public string Message { get; }

        public GraphError(string message)
        {
            Message = message;
        }
    }

    public class QueryExecutor
    {
        private readonly Schema _schema;
        private readonly QueryExecutorOptions _options;
        private readonly List<IMiddleware> _middlewares;

        public QueryExecutor(Schema schema, QueryExecutorOptions options)
        {
            _schema = schema;
            _options = options;

            _middlewares = options.MiddlewareTypes.Select(options.Resolver).Cast<IMiddleware>().ToList();
        }

        public async Task<ExecutionResult> ExecuteAsync(string query)
        {
            var lexer = new Lexer();
            var parser = new Parser(lexer);
            var ast = parser.Parse(new Source(query));

            var firstDefinition = (GraphQLOperationDefinition)ast.Definitions.First();

            ObjectGraphType initialObjectType;
            object initialObjectValue;
            switch (firstDefinition.Operation)
            {
                case OperationType.Query:
                    initialObjectType = _schema.Query;
                    initialObjectValue = _options.Resolver(_schema.Query.ClrType);
                    break;
                case OperationType.Mutation:
                    initialObjectType = _schema.Mutation;
                    initialObjectValue = _options.Resolver(_schema.Mutation.ClrType);
                    break;
                case OperationType.Subscription:
                    throw new NotSupportedException("Subscriptions are currently not supported.");
                default:
                    throw new NotSupportedException();
            }

            var executionContext = new QueryExecutionContext();
            var firstSelectionSet = firstDefinition.SelectionSet;
            var data = await ExecuteSelectionSetAsync(executionContext, firstSelectionSet, initialObjectType, initialObjectValue);
            var result = new JObject
            {
                { "data", data }
            };

            if (executionContext.Errors.Any())
            {
                var errors = new JArray();
                foreach (var error in executionContext.Errors.Select(x => new JObject(new JProperty("message", x.Message))))
                {
                    errors.Add(error);
                }
                result.Add("errors", errors);
            }

            return new ExecutionResult { Data = result };
        }

        //private static readonly Dictionary<ASTNodeKind, Func<ASTNode, GraphType, object, Task<JToken>>> ProcessorMap = new Dictionary<ASTNodeKind, Func<ASTNode, GraphType, object, Task<JToken>>>
        //{
        //    { ASTNodeKind.Document, CreateProcessor<GraphQLSelectionSet>(ExecuteNodeAsync)},
        //    { ASTNodeKind.SelectionSet, CreateProcessor<GraphQLSelectionSet>(ExecuteSelectionSetAsync)},
        //    { ASTNodeKind.OperationDefinition, CreateProcessor<GraphQLFieldSelection>(ExecuteOperationAsync) }
        //};

        //private static async Task<JToken> ExecuteOperationAsync(GraphQLFieldSelection ast, GraphType objectType, object objectValue)
        //{
        //    return await ExecuteNodeAsync(ast.SelectionSet, objectType, objectValue);
        //}

        //private static Func<ASTNode, GraphType, object, Task<JToken>> CreateProcessor<TNodeType>(Func<TNodeType, GraphType, object, Task<JToken>> process) where TNodeType : ASTNode
        //{
        //    return (node, objectType, objectValue) => process((TNodeType)node, objectType, objectValue);
        //}

        //private static async Task<JToken> ExecuteNodeAsync(ASTNode ast, GraphType objectType, object objectValue)
        //{
        //    return await ProcessorMap[ast.Kind](ast, objectType, objectValue);
        //}

        private async Task<JToken> ExecuteSelectionSetAsync(QueryExecutionContext executionContext, GraphQLSelectionSet ast, ObjectGraphType objectType, object objectValue)
        {
            var groupedFieldSet = CollectFields(objectType, ast.Selections);

            // TODO support both parallell and serial execution 
            var result = new JObject();
            foreach (var fieldSet in groupedFieldSet)
            {
                var responseKey = fieldSet.Key;
                // TODO add support for fragments
                var field = fieldSet.Value.First();
                var fieldName = field.Name.Value;
                var fieldType = objectType.GetFieldType(fieldName);
                // TODO check that field exists (not null). If null ignore
                var responseValue = await ExecuteFieldAsync(executionContext, objectType, objectValue, fieldType, field);
                result.Add(responseKey, responseValue);
            }

            return result;
        }

        private static Dictionary<string, List<GraphQLFieldSelection>> CollectFields(GraphType objectType, IEnumerable<ASTNode> selections)
        {
            // Not supporting fragments now
            return selections.Cast<GraphQLFieldSelection>().GroupBy(x => x.Alias?.Value ?? x.Name.Value).ToDictionary(x => x.Key, x => x.ToList());
        }

        public interface IMiddleware
        {
            Task<object> Resolve(FieldResolveContext fieldContext, FieldResolver next);
        }

        private async Task<JToken> ExecuteFieldAsync(QueryExecutionContext executionContext, ObjectGraphType objectType, object objectValue, GraphType fieldType, GraphQLFieldSelection field)
        {
            var argumentValues = CoerceArgumentValues(objectType, field);

            object resolvedValue = null;

            // TODO do not catch errors if this is a non nullable field 
            try
            {
                var graphFieldInfo = objectType.GetFieldInfo(field.Name.Value);
                var fieldResolveContext = new FieldResolveContext(objectValue, argumentValues, graphFieldInfo);

                FieldResolver exec = context => graphFieldInfo.Resolver(context);

                // TODO create middleware once by parameterizing the last exec/next func
                foreach (var middleware in Enumerable.Reverse(_middlewares))
                {
                    var middleware1 = middleware;
                    var exec1 = exec;
                    exec = context => middleware1.Resolve(context, exec1);
                }

                resolvedValue = await exec(fieldResolveContext);
                
            }
            catch (FieldErrorException ex)
            {
                executionContext.AddError(new GraphError(ex.Message));
            }
            
            return await CompleteValue(executionContext, field, fieldType, resolvedValue);
        }

        private static Dictionary<string, object> CoerceArgumentValues(ObjectGraphType objectType, GraphQLFieldSelection field)
        {
            var coercedValues = new Dictionary<string, object>();
            var argumentValues = field.Arguments.ToDictionary(x => x.Name.Value);
            var fieldName = field.Name;
            var argumentDefinitions = objectType.GetArgumentDefinitions(fieldName.Value);
            foreach (var argumentDefinition in argumentDefinitions)
            {
                var argumentName = argumentDefinition.Name;
                var argumentType = argumentDefinition.Type;
                if (!argumentValues.ContainsKey(argumentName))
                {
                    if (argumentDefinition.HasDefaultValue)
                    {
                        coercedValues[argumentName] = argumentDefinition.DefaultValue;
                    }
                    // TODO check null
                }
                else
                {
                    var value = argumentValues[argumentName];
                    coercedValues[argumentName] = CoerceInputValue(value, argumentType);
                }
            }

            return coercedValues;
        }

        private static object CoerceInputValue(GraphQLArgument value, GraphType argumentType)
        {
            return argumentType.CoerceInputValue(value.Value);
        }

        private async Task<JToken> CompleteValue(QueryExecutionContext executionContext, GraphQLFieldSelection field, GraphType fieldType, object resolvedValue)
        {
            if (fieldType is ObjectGraphType)
            {
                return await ExecuteSelectionSetAsync(executionContext, field.SelectionSet, (ObjectGraphType) fieldType, resolvedValue);
            }

            if (fieldType is ListGraphType)
            {
                var listFieldType = (ListGraphType) fieldType;
                
                var resolvedCollection = (IEnumerable<object>)resolvedValue;
                if (resolvedValue == null)
                {
                    return null;
                }

                var resultArray = new JArray();
                foreach (var resolvedItem in resolvedCollection)
                {
                    var completedValue =
                        await CompleteValue(executionContext, field, listFieldType.ItemType, resolvedItem);
                    resultArray.Add(completedValue);
                }

                return resultArray;
            }

            return new JValue(resolvedValue);
        }
    }
}
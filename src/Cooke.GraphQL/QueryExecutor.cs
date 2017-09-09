using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cooke.GraphQL.Introspection;
using Cooke.GraphQL.Types;
using GraphQLParser;
using GraphQLParser.AST;
using Newtonsoft.Json.Linq;

namespace Cooke.GraphQL
{
    public class QueryExecutor
    {
        private readonly Schema _schema;
        private readonly QueryExecutorOptions _options;
        private readonly List<IMiddleware> _middlewares;
        private IntrospectionQuery _introspectionObjectValue;
        private ObjectGraphType _introspectionObjectType;

        public QueryExecutor(Schema schema, QueryExecutorOptions options)
        {
            _schema = schema;
            _options = options;
            var introspectionSchema = new SchemaBuilder().UseQuery<IntrospectionQuery>().Build();
            _introspectionObjectValue = new IntrospectionQuery(schema, introspectionSchema);
            _introspectionObjectType = introspectionSchema.Query;

            _middlewares = options.MiddlewareTypes.Select(options.Resolver).Cast<IMiddleware>().ToList();
        }

        public async Task<JObject> ExecuteAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new JObject
                {
                    { "data", null },
                    { "errors", new JArray(new JValue("A query must be specified and must be a string."))}
                };
            }

            var lexer = new Lexer();
            var parser = new Parser(lexer);
            var ast = parser.Parse(new Source(query));

            var firstOperationDefinition = (GraphQLOperationDefinition)ast.Definitions.First();

            ObjectGraphType initialObjectType;
            object initialObjectValue;
            bool exposeIntrospection = true;
            switch (firstOperationDefinition.Operation)
            {
                case OperationType.Query:
                    initialObjectType = _schema.Query;
                    initialObjectValue = _options.Resolver(_schema.Query.ClrType);
                    break;
                case OperationType.Mutation:
                    initialObjectType = _schema.Mutation;
                    initialObjectValue = _options.Resolver(_schema.Mutation.ClrType);
                    exposeIntrospection = false;
                    break;
                case OperationType.Subscription:
                    throw new NotSupportedException("Subscriptions are currently not supported.");
                default:
                    throw new NotSupportedException();
            }

            var executionContext = new QueryExecutionContext();
            var firstSelectionSet = firstOperationDefinition.SelectionSet;
            var data = await ExecuteSelectionSetAsync(executionContext, firstSelectionSet, initialObjectType, initialObjectValue, exposeIntrospection);
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

            return result;
        }

        private async Task<JToken> ExecuteSelectionSetAsync(QueryExecutionContext executionContext, GraphQLSelectionSet ast, ComplexGraphType objectType, object objectValue, bool exposeIntrospection = false)
        {
            var groupedFieldSet = CollectFields(ast.Selections);

            // TODO support both parallell and serial execution 
            var result = new JObject();
            foreach (var fieldSet in groupedFieldSet)
            {
                var responseKey = fieldSet.Key;
                // TODO add support for fragments
                var field = fieldSet.Value.First();
                var fieldName = field.Name.Value;

                // Special handling of introspection fields
                var localObjectType = objectType;
                var localObjectValue = objectValue;
                if (exposeIntrospection && fieldName.StartsWith("__"))
                {
                    localObjectType = _introspectionObjectType;
                    localObjectValue = _introspectionObjectValue;
                }

                var fieldType = localObjectType.GetFieldType(fieldName);
                // TODO check that field exists (not null). If null ignore
                var responseValue = await ExecuteFieldAsync(executionContext, localObjectType, localObjectValue, fieldType, field);
                result.Add(responseKey, responseValue);
            }

            return result;
        }

        private static Dictionary<string, List<GraphQLFieldSelection>> CollectFields(IEnumerable<ASTNode> selections)
        {
            // TODO add support for fragments
            return selections.Cast<GraphQLFieldSelection>().GroupBy(x => x.Alias?.Value ?? x.Name.Value).ToDictionary(x => x.Key, x => x.ToList());
        }

        public interface IMiddleware
        {
            Task<object> Resolve(FieldResolveContext fieldContext, FieldResolver next);
        }

        private async Task<JToken> ExecuteFieldAsync(QueryExecutionContext executionContext, ComplexGraphType objectType, object objectValue, GraphType fieldType, GraphQLFieldSelection field)
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
                executionContext.AddError(new GraphQLError(ex.Message));
            }
            
            return await CompleteValue(executionContext, field, fieldType, resolvedValue);
        }

        private static Dictionary<string, object> CoerceArgumentValues(ComplexGraphType objectType, GraphQLFieldSelection field)
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

        private async Task<JToken> CompleteValue(QueryExecutionContext executionContext, GraphQLFieldSelection field, GraphType fieldType, object result)
        {
            if (fieldType is NotNullGraphType nonNullGraphType)
            {
                var completeValue = await CompleteValue(executionContext, field, nonNullGraphType.ItemType, result);
                if (completeValue == null)
                {
                    throw new FieldErrorException("Non null field resolved to null");
                }
                return completeValue;
            }

            if (result == null)
            {
                return null;
            }

            if (fieldType is ComplexGraphType objectGraphType)
            {
                return await ExecuteSelectionSetAsync(executionContext, field.SelectionSet, objectGraphType, result);
            }

            if (fieldType is ListGraphType type)
            {
                var listFieldType = type;

                var resolvedCollection = (IEnumerable)result;

                var resultArray = new JArray();
                foreach (var resolvedItem in resolvedCollection)
                {
                    var completedValue =
                        await CompleteValue(executionContext, field, listFieldType.ItemType, resolvedItem);
                    resultArray.Add(completedValue);
                }

                return resultArray;
            }

            if (fieldType is ScalarGraphType scalarGraphType)
            {
                return scalarGraphType.CoerceResultValue(result);
            }

            throw new NotSupportedException("The given field type is not supported");
        }
    }
}
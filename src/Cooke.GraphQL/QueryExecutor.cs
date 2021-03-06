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

    public class QueryErrorException : Exception
    {
        public QueryErrorException(string message) : base(message)
        {
        }
    }

    public class QueryExecutor
    {
        private readonly Schema _schema;
        private readonly QueryExecutorOptions _options;
        private readonly List<IMiddleware> _middlewares;
        private readonly IntrospectionQuery _introspectionObjectValue;
        private readonly ObjectType _introspectionObjectType;
        private readonly Schema _introspectionSchema;

        public QueryExecutor(Schema schema, QueryExecutorOptions options)
        {
            _schema = schema;
            _options = options;
            _introspectionSchema = new SchemaBuilder().UseQuery<IntrospectionQuery>().Build();
            _introspectionObjectValue = new IntrospectionQuery(schema, _introspectionSchema);
            _introspectionObjectType = _introspectionSchema.Query;

            _middlewares = options.MiddlewareTypes.Select(options.Resolver).Cast<IMiddleware>().ToList();
        }

        public Task<JObject> ExecuteRequestAsync(string queryDocument)
        {
            return ExecuteRequestAsync(queryDocument, null, null);
        }

        public async Task<JObject> ExecuteRequestAsync(string documentSource, string operationName, JObject variableValues)
        {
            if (string.IsNullOrWhiteSpace(documentSource))
            {
                return new JObject
                {
                    { "data", null },
                    { "errors", new JArray(new JValue("A query must be specified and must be a string."))}
                };
            }

            var lexer = new Lexer();
            var parser = new Parser(lexer);
            var document = parser.Parse(new Source(documentSource));

            var fragmentDefinitions = document.Definitions.OfType<GraphQLFragmentDefinition>().ToDictionary(x => x.Name.Value);

            var operation = GetOperation(document, operationName);
            var coercedVariableValues = CoerceVariableValues(operation, variableValues ?? new JObject());

            ObjectType initialObjectType;
            object initialObjectValue;
            bool exposeIntrospection = true;
            switch (operation.Operation)
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

            var executionContext = new QueryExecutionContext(fragmentDefinitions, coercedVariableValues);
            var firstSelectionSet = operation.SelectionSet;
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

        private Dictionary<string, object> CoerceVariableValues(GraphQLOperationDefinition operation, JObject variableValues)
        {
            var coercedValues = new Dictionary<string, object>();
            var variableDefinitions = operation.VariableDefinitions ?? Enumerable.Empty<GraphQLVariableDefinition>();
            foreach (var variableDefinition in variableDefinitions)
            {
                var variableName = variableDefinition.Variable.Name.Value;
                var variableType = ParseType(variableDefinition.Type);

                if (!variableValues.TryGetValue(variableName, out JToken value))
                {
                    if (variableType.Kind == __TypeKind.Non_Null && variableDefinition.DefaultValue == null)
                    {
                        throw new QueryErrorException($"Non nullable variable '{variableName}' is missing a value.");
                    }
                    else
                    {
                        coercedValues[variableName] = variableDefinition.DefaultValue;
                    }
                }
                else
                {
                    coercedValues[variableName] = variableType.CoerceInputVariableValue(value);
                }
            }

            return coercedValues;
        }

        private BaseType ParseType(GraphQLType variableDefinitionType)
        {
            switch (variableDefinitionType)
            {
                case GraphQLListType graphQLListType:
                    return new ListType { ItemType = ParseType(graphQLListType.Type)}; ;
                case GraphQLNamedType graphQLNamedType:
                    return _schema.Types.First(x => x.Name == graphQLNamedType.Name.Value);
                case GraphQLNonNullType graphQLNonNullType:
                    return new NonNullType { ItemType = ParseType(graphQLNonNullType.Type) };
                    default:
                        throw new NotSupportedException();
            }
        }

        private GraphQLOperationDefinition GetOperation(GraphQLDocument document, string operationName)
        {
            var operationDefinitions = document.Definitions.OfType<GraphQLOperationDefinition>().ToArray();
            if (operationName == null)
            {
                if (operationDefinitions.Length != 1)
                {
                    throw new QueryErrorException(
                        "An operation name is required since there are several operations defined in the query document.");
                }

                return operationDefinitions.First();
            }
            else
            {
                var selectedOperation = operationDefinitions.FirstOrDefault(x => x.Name.Value == operationName);
                if (selectedOperation == null)
                {
                    throw new QueryErrorException($"No operation named '{operationName}' exists in the query document.");
                }
                return selectedOperation;
            }
        }

        private async Task<JToken> ExecuteSelectionSetAsync(QueryExecutionContext executionContext, GraphQLSelectionSet ast, ComplexBaseType objectType, object objectValue, bool exposeIntrospection = false)
        {
            var groupedFieldSet = CollectFields(objectType, executionContext, ast.Selections);

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

        private Dictionary<string, List<GraphQLFieldSelection>> CollectFields(BaseType objectType, QueryExecutionContext context, IEnumerable<ASTNode> selections, HashSet<string> visitedFragments = null)
        {
            visitedFragments = visitedFragments ?? new HashSet<string>();
            var groupedFields = new Dictionary<string, List<GraphQLFieldSelection>>();

            foreach (var selection in selections)
            {
                if (selection is GraphQLFieldSelection fieldSelection)
                {
                    var responseKey = fieldSelection.Alias?.Value ?? fieldSelection.Name.Value;
                    if (!groupedFields.ContainsKey(responseKey))
                    {
                        groupedFields[responseKey] = new List<GraphQLFieldSelection>();
                    }

                    var groupForResponseKey = groupedFields[responseKey];
                    groupForResponseKey.Add(fieldSelection);
                }
                else if (selection is GraphQLFragmentSpread fragmentSelection)
                {
                    var fragmentSpreadName = fragmentSelection.Name.Value;
                    if (visitedFragments.Contains(fragmentSpreadName))
                    {
                        continue;
                    }

                    visitedFragments.Add(fragmentSpreadName);

                    if (!context.FragmentDefinitions.ContainsKey(fragmentSpreadName))
                    {
                        continue;
                    }

                    var fragment = context.FragmentDefinitions[fragmentSpreadName];
                    var fragmentTypeName = fragment.TypeCondition.Name.Value;
                    var fragmentType = _schema.Types.Concat(_introspectionSchema.Types).First(x => x.Name == fragmentTypeName);
                    if (!DoesFragmentTypeApply(objectType, fragmentType))
                    {
                        continue;
                    }

                    var fragmentSelectionSet = fragment.SelectionSet.Selections;
                    var fragmentGroupedFieldSet = CollectFields(objectType, context, fragmentSelectionSet, visitedFragments);
                    foreach (var fragmentGroup in fragmentGroupedFieldSet)
                    {
                        var responseKey = fragmentGroup.Key;
                        if (!groupedFields.ContainsKey(responseKey))
                        {
                            groupedFields[responseKey] = new List<GraphQLFieldSelection>();
                        }
                        var groupForResponseKey = groupedFields[responseKey];
                        groupForResponseKey.AddRange(fragmentGroup.Value);
                    }
                }
                else
                {
                    // TODO
                    throw new NotSupportedException();
                }
            }

            return groupedFields;

            // var groupedFields = selections.Cast<GraphQLFieldSelection>().GroupBy(x => x.Alias?.Value ?? x.Name.Value).ToDictionary(x => x.Key, x => x.ToList());
        }

        private static bool DoesFragmentTypeApply(BaseType objectType, BaseType fragmentType)
        {
            if (fragmentType.Kind == __TypeKind.Object)
            {
                return fragmentType == objectType;
            }
            else if (fragmentType.Kind == __TypeKind.Interface)
            {
                return objectType is ObjectType objType && objType.Interfaces.Any(y => y.Name == fragmentType.Name);
            }
            
            // TODO support union types
            throw new NotImplementedException();
        }

        public interface IMiddleware
        {
            Task<object> Resolve(FieldResolveContext fieldContext, FieldResolver next);
        }

        private async Task<JToken> ExecuteFieldAsync(QueryExecutionContext executionContext, ComplexBaseType objectType, object objectValue, BaseType fieldType, GraphQLFieldSelection field)
        {
            var argumentValues = CoerceArgumentValues(objectType, field, executionContext);

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
                executionContext.AddError(new QueryError(ex.Message));
            }
            
            return await CompleteValue(executionContext, field, fieldType, resolvedValue);
        }

        private static Dictionary<string, object> CoerceArgumentValues(ComplexBaseType objectType, GraphQLFieldSelection field, QueryExecutionContext executionContext)
        {
            var coercedValues = new Dictionary<string, object>();
            var argumentValues = field.Arguments.ToDictionary(x => x.Name.Value);
            var fieldName = field.Name;
            var argumentDefinitions = objectType.GetArgumentDefinitions(fieldName.Value);
            foreach (var argumentDefinition in argumentDefinitions)
            {
                var argumentName = argumentDefinition.Name;
                var argumentType = argumentDefinition.Type;
                var defaultValue = argumentDefinition.DefaultValue;
                if (argumentValues.ContainsKey(argumentName) && argumentValues[argumentName].Value is GraphQLVariable variable)
                {
                    var variableName = variable.Name.Value;
                    if (executionContext.Variables.ContainsKey(variableName))
                    {
                        coercedValues[argumentName] = executionContext.Variables[variableName];
                    }
                    else if (argumentDefinition.HasDefaultValue)
                    {
                        coercedValues[argumentName] = defaultValue;
                    }
                    else if (argumentType.Kind == __TypeKind.Non_Null)
                    {
                        throw new FieldErrorException(
                            $"Variable '{variableName}' must be given a value since it cannot be null");
                    }
                }
                else if (!argumentValues.ContainsKey(argumentName))
                {
                    if (argumentDefinition.HasDefaultValue)
                    {
                        coercedValues[argumentName] = defaultValue;
                    }
                    else if (argumentType.Kind == __TypeKind.Non_Null)
                    {
                        throw new FieldErrorException(
                            $"Argument '{argumentName}' must be given a value since it cannot be null");
                    }
                }
                else
                {
                    try
                    {
                        var coercedValue = CoerceInputValue(argumentValues[argumentName], argumentType);
                        coercedValues[argumentName] = coercedValue;
                    }
                    catch (TypeCoercionException)
                    {
                        throw new FieldErrorException($"Input coercion failed for argument '{argumentName}'");
                    }
                }
            }

            return coercedValues;
        }

        private static object CoerceInputValue(GraphQLArgument value, BaseType argumentType)
        {
            return argumentType.CoerceInputLiteralValue(value.Value);
        }

        private async Task<JToken> CompleteValue(QueryExecutionContext executionContext, GraphQLFieldSelection field, BaseType fieldType, object result)
        {
            if (fieldType is NonNullType nonNullGraphType)
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

            if (fieldType is ComplexBaseType objectGraphType)
            {
                return await ExecuteSelectionSetAsync(executionContext, field.SelectionSet, objectGraphType, result);
            }

            if (fieldType is ListType type)
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

            if (fieldType is ScalarBaseType scalarGraphType)
            {
                return scalarGraphType.CoerceResultValue(result);
            }

            throw new NotSupportedException("The given field type is not supported");
        }
    }
}
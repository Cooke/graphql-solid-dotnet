using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQLParser;
using GraphQLParser.AST;
using Newtonsoft.Json.Linq;

namespace Tests
{
    public class QueryExecutorOptions
    {
        public Func<Type, object> Resolver = t => Activator.CreateInstance(t);
    }

    public class QueryExecutor
    {
        private readonly Schema _schema;
        private readonly QueryExecutorOptions _options;

        public QueryExecutor(Schema schema, QueryExecutorOptions options)
        {
            _schema = schema;
            _options = options;
        }

        public async Task<ExecutionResult> ExecuteAsync(string query)
        {
            var lexer = new Lexer();
            var parser = new Parser(lexer);
            var ast = parser.Parse(new Source(query));

            var initialObjectType = _schema.Query;
            var initialObjectValue = _options.Resolver(_schema.Query.ClrType);

            if (initialObjectType == null)
            {
                throw new ArgumentException();
            }

            var firstSelectionSet = ast.Definitions.OfType<GraphQLOperationDefinition>().First().SelectionSet;
            var data = await ExecuteSelectionSetAsync(firstSelectionSet, initialObjectType, initialObjectValue);
            return new ExecutionResult { Data = new JObject { { "data", data } } };
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

        private static async Task<JToken> ExecuteSelectionSetAsync(GraphQLSelectionSet ast, ObjectGraphType objectType, object objectValue)
        {
            var groupedFieldSet = CollectFields(objectType, ast.Selections);

            var result = new JObject();
            foreach (var fieldSet in groupedFieldSet)
            {
                var responseKey = fieldSet.Key;
                // TODO add support for fragments
                var field = Enumerable.First<GraphQLFieldSelection>(fieldSet.Value);
                var fieldName = field.Name.Value;
                var fieldType = objectType.GetFieldType(fieldName);
                // TODO check that field exists (not null). If null ignore
                var responseValue = await ExecuteFieldAsync(objectType, objectValue, fieldType, field);
                result.Add(responseKey, responseValue);
            }

            return result;
        }

        private static Dictionary<string, List<GraphQLFieldSelection>> CollectFields(GraphType objectType, IEnumerable<ASTNode> selections)
        {
            // Not supporting fragments now
            return selections.Cast<GraphQLFieldSelection>().GroupBy(x => x.Alias?.Value ?? x.Name.Value).ToDictionary(x => x.Key, x => x.ToList());
        }

        private static async Task<JToken> ExecuteFieldAsync(ObjectGraphType objectType, object objectValue, GraphType fieldType, GraphQLFieldSelection field)
        {
            var argumentValues = CoerceArgumentValues(objectType, field);
            var resolvedValue = await objectType.ResolveAsync(objectValue, field.Name.Value, argumentValues);
            
            return await CompleteValue(field, fieldType, resolvedValue);
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
                    // TODO add coercing

                    coercedValues[argumentName] = CoerceInputValue(value, argumentType);
                }
            }

            return coercedValues;
        }

        private static object CoerceInputValue(GraphQLArgument value, GraphType argumentType)
        {
            return argumentType.InputCoerceValue(value.Value);
        }

        private static async Task<JToken> CompleteValue(GraphQLFieldSelection field, GraphType fieldType, object resolvedValue)
        {
            if (fieldType is ObjectGraphType)
            {
                return await ExecuteSelectionSetAsync(field.SelectionSet, (ObjectGraphType) fieldType, resolvedValue);
            }
            else if (fieldType is ListGraphType)
            {
                var listFieldType = (ListGraphType) fieldType;
                var resultArray = new JArray();
                
                var resolvedCollection = (IEnumerable<object>)resolvedValue;
                foreach (var resolvedItem in resolvedCollection)
                {
                    var completedValue = await CompleteValue(field, listFieldType.ItemType, resolvedItem);
                    resultArray.Add(completedValue);
                }

                return resultArray;
            }
            else
            {
                return new JValue(resolvedValue);
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GraphQLParser;
using GraphQLParser.AST;
using Xunit;
using Newtonsoft.Json.Linq;

namespace Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            //var graphqlSchema = new GraphqlSchemaBuilder()
            //    .UseQuery<Query>()
            //    .Build();

            var schemaType = new GraphqlType(typeof(Schema));
            var schemaInstance = new Schema();
            var query = @"{ Users { Username } }";

            var graphqlExecutor = new GraphqlExecutor();
            var result = graphqlExecutor.ExecuteAsync(query, schemaType, schemaInstance).Result;

            var expectedData = JObject.Parse("{ data: { users: [ 'henrik' ] } }");
            Assert.Equal(expectedData, result.Data);
        }
    }

    //public class GraphqlSchemaBuilder
    //{
    //    private Type _queryType;

    //    public GraphqlSchemaBuilder UseQuery<T>()
    //    {
    //        _queryType = typeof(T);
    //        return this;
    //    }

    //    public GraphqlType Build()
    //    {
    //        var makeGenericType = typeof(RootType<>).MakeGenericType(_queryType);
    //        return new GraphqlType(makeGenericType);
    //    }
        
    //    private class RootType<TQueryType>
    //    {
    //        // ReSharper disable once UnassignedGetOnlyAutoProperty
    //        public TQueryType Query { get; }
    //    }
    //}

    public class GraphFieldInfo
    {
        private readonly MemberInfo _memberInfo;
        private readonly GraphqlType graphType;
        private readonly Func<object, Task<object>> resolver;

        public string Name => _memberInfo.Name;
        public GraphqlType Type => graphType;

        public GraphFieldInfo(MemberInfo memberInfo)
        {
            _memberInfo = memberInfo;
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Property:
                    var propertyInfo = (PropertyInfo)memberInfo;
                    graphType = new GraphqlType(propertyInfo.PropertyType);
                    resolver = x => Task.FromResult(propertyInfo.GetValue(x));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public Task<object> ResolveAsync(object objectValue)
        {
            return resolver(objectValue);
        }
    }

    public class GraphqlType
    {
        private readonly Type _clrType;
        private readonly Dictionary<string, GraphFieldInfo> _fields;

        public GraphqlType(Type clrType)
        {
            if (typeof(Task).IsAssignableFrom(clrType))
            {
                clrType = clrType.GetGenericArguments().Single();
            }

            _clrType = clrType;

            if (TypeHelper.IsList(clrType))
            {
                var enumerableType = _clrType.GetInterfaces().First(x => x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                IsList = true;
                InnerType = new GraphqlType(enumerableType.GetGenericArguments().Single());
            }
            else if (clrType.GetTypeInfo().IsPrimitive  || clrType == typeof(string))
            {

            }
            else
            {
                var memberInfos = clrType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                _fields = memberInfos.Select(x => new GraphFieldInfo(x)).ToDictionary(x => x.Name);
            }
        }

        public bool IsObject => _clrType.GetTypeInfo().IsClass;

        public bool IsList { get; }

        public Type ClrType => _clrType;

        public GraphqlType InnerType { get; }
        
        public async Task<object> ResolveAsync(object objectValue, string fieldName)
        {
            return await _fields[fieldName].ResolveAsync(objectValue);
        }

        public GraphqlType GetFieldType(string fieldName)
        {
            return _fields[fieldName].Type;
;        }
    }

    public class TypeHelper
    {
        public static bool IsList(Type clrType)
        {
            return typeof(IEnumerable<object>).IsAssignableFrom(clrType);
        }
    }

    public class GraphqlExecutor
    {
        public async Task<ExecutionResult> ExecuteAsync(string query, GraphqlType schemaType, object schemaInstance)
        {
            var lexer = new Lexer();
            var parser = new Parser(lexer);
            var ast = parser.Parse(new Source(query));
            
            var initialObjectType = schemaType.GetFieldType("Query");
            var initialObjectValue = await schemaType.ResolveAsync(schemaInstance, "Query");

            if (!initialObjectType.IsObject)
            {
                throw new ArgumentException();
            }

            var firstSelectionSet = ast.Definitions.OfType<GraphQLOperationDefinition>().First().SelectionSet;
            var data = await ExecuteSelectionSetAsync(firstSelectionSet, initialObjectType, initialObjectValue);
            return new ExecutionResult { Data = data };
        }

        private static readonly Dictionary<ASTNodeKind, Func<ASTNode, GraphqlType, object, Task<JToken>>> ProcessorMap = new Dictionary<ASTNodeKind, Func<ASTNode, GraphqlType, object, Task<JToken>>>
        {
            { ASTNodeKind.Document, CreateProcessor<GraphQLSelectionSet>(ExecuteNodeAsync)},
            { ASTNodeKind.SelectionSet, CreateProcessor<GraphQLSelectionSet>(ExecuteSelectionSetAsync)},
            { ASTNodeKind.OperationDefinition, CreateProcessor<GraphQLFieldSelection>(ExecuteOperationAsync) }
        };

        private static async Task<JToken> ExecuteOperationAsync(GraphQLFieldSelection ast, GraphqlType objectType, object objectValue)
        {
            return await ExecuteNodeAsync(ast.SelectionSet, objectType, objectValue);
        }

        private static Func<ASTNode, GraphqlType, object, Task<JToken>> CreateProcessor<TNodeType>(Func<TNodeType, GraphqlType, object, Task<JToken>> process) where TNodeType : ASTNode
        {
            return (node, objectType, objectValue) => process((TNodeType)node, objectType, objectValue);
        }

        private static async Task<JToken> ExecuteNodeAsync(ASTNode ast, GraphqlType objectType, object objectValue)
        {
            return await ProcessorMap[ast.Kind](ast, objectType, objectValue);
        }

        private static async Task<JToken> ExecuteSelectionSetAsync(GraphQLSelectionSet ast, GraphqlType objectType, object objectValue)
        {
            var groupedFieldSet = CollectFields(objectType, ast.Selections);

            var result = new JObject();
            foreach (var fieldSet in groupedFieldSet)
            {
                var responseKey = fieldSet.Key;
                // TODO add support for fragments
                var field = fieldSet.Value.First();
                var fieldName = field.Name.Value;
                var fieldType = objectType.GetFieldType(fieldName);
                // TODO check that field exists (not null). If null ignore
                var responseValue = await ExecuteFieldAsync(objectType, objectValue, field, fieldType);
                result.Add(responseKey, responseValue);
            }

            return result;
        }

        private static Dictionary<string, List<GraphQLFieldSelection>> CollectFields(GraphqlType objectType, IEnumerable<ASTNode> selections)
        {
            // Not supporting fragments now
            return selections.Cast<GraphQLFieldSelection>().GroupBy(x => x.Alias?.Value ?? x.Name.Value).ToDictionary(x => x.Key, x => x.ToList());
        }

        private static async Task<JToken> ExecuteFieldAsync(GraphqlType objectType, object objectValue, GraphQLFieldSelection field, GraphqlType fieldType)
        {
            var resolvedValue = await objectType.ResolveAsync(objectValue, field.Name.Value);
            
            return await CompleteValue(field, fieldType, resolvedValue);
        }

        private static async Task<JToken> CompleteValue(GraphQLFieldSelection field, GraphqlType fieldType, object resolvedValue)
        {
            if (fieldType.IsObject)
            {
                return await ExecuteSelectionSetAsync(field.SelectionSet, fieldType, resolvedValue);
            }
            else if (fieldType.IsList)
            {
                var resultArray = new JArray();

                var resolvedCollection = (IEnumerable<object>)resolvedValue;
                foreach (var resolvedItem in resolvedCollection)
                {
                    var completedValue = await CompleteValue(field, fieldType.InnerType, resolvedItem);
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
    
    public class ExecutionResult
    {
        public object Data { get; set; }
    }
}
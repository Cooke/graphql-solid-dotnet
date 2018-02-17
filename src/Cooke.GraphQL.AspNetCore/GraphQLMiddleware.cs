using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cooke.GraphQL.AspNetCore
{
    public class GraphQLMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly QueryExecutor _queryExecutor;

        public GraphQLMiddleware(RequestDelegate next, QueryExecutor queryExecutor)
        {
            _next = next;
            _queryExecutor = queryExecutor;
        }

        // TODO this impl was thrown together just to make it work. Go over it and think it through.
        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/graphql"))
            {
                var queryRequest = await GetQueryRequest(context);

                var executionResult = await _queryExecutor.ExecuteRequestAsync(queryRequest.Query, queryRequest.OperationName, queryRequest.Variables);
                var serializeObject = JsonConvert.SerializeObject(executionResult);
                await context.Response.WriteAsync(serializeObject);
            }
            else
            {
                await _next(context);
            }
        }

        private static async Task<QueryRequest> GetQueryRequest(HttpContext context)
        {
            if (!MediaTypeHeaderValue.TryParse(context.Request.ContentType, out var contentMediaType))
            {
                return null;
            }

            var streamReader = new StreamReader(context.Request.Body, Encoding.GetEncoding(contentMediaType.CharSet ?? "utf-8"));

            if (contentMediaType.MediaType == "application/json")
            {
                var inputToken = await JToken.ReadFromAsync(new JsonTextReader(streamReader));
                return new QueryRequest
                {
                    OperationName = inputToken["operationName"]?.Value<string>(),
                    Query = inputToken["query"]?.Value<string>(),
                    Variables = (JObject) inputToken["variables"]
                };
            }

            if (contentMediaType.MediaType == "application/graphql")
            {
                return new QueryRequest {Query = await streamReader.ReadToEndAsync()};
            }

            return null;
        }

        private class QueryRequest
        {
            public string Query { get; set; }

            public JObject Variables { get; set; }

            public string OperationName { get; set; }
        }
    }
}
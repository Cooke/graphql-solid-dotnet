using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

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
                var query = await GetQuery(context);

                var executionResult = await _queryExecutor.ExecuteAsync(query);
                var serializeObject = JsonConvert.SerializeObject(executionResult);
                await context.Response.WriteAsync(serializeObject);
            }
            else
            {
                await _next(context);
            }
        }

        private static async Task<string> GetQuery(HttpContext context)
        {
            MediaTypeHeaderValue contentMediaType;
            if (!MediaTypeHeaderValue.TryParse(context.Request.ContentType, out contentMediaType))
            {
                return null;
            }

            var requestBody = context.Request.Body;
            var streamReader = new StreamReader(requestBody, Encoding.GetEncoding(contentMediaType.CharSet ?? "utf-8"));
            var bodyString = await streamReader.ReadToEndAsync();

            if (contentMediaType.MediaType == "application/json")
            {
                var requestJson = JsonConvert.DeserializeObject<IDictionary<string, string>>(bodyString);
                return requestJson != null && requestJson.ContainsKey("query") ? requestJson["query"] : null;
            }

            if (contentMediaType.MediaType == "application/graphql")
            {
                return bodyString;
            }

            return null;
        }
    }
}